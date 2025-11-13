using AdUserStatus.Models;
using ClosedXML.Excel;
using ExcelDataReader;
using System.Data;
using System.Text;
using System.Xml.Linq;

namespace AdUserStatus.Services
{
    public static class ExcelService
    {
        // === PUBLIC API ===

        public static List<UserDto> ReadUsers(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");

            var ext = Path.GetExtension(path).ToLowerInvariant();

            // 1) Try real Excel first if the extension suggests it
            if (ext == ".xls" || ext == ".xlsx")
            {
                try { return ReadRealExcel(path); }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("Invalid file signature", StringComparison.OrdinalIgnoreCase))
                        throw;
                }
            }

            // 2) Sniff first bytes to decide XML vs text
            using var fs = File.OpenRead(path);
            var head = new byte[Math.Min(256, fs.Length)];
            _ = fs.Read(head, 0, head.Length);
            var headStr = Encoding.UTF8.GetString(head).TrimStart('\uFEFF', ' ', '\t', '\r', '\n');

            if (headStr.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) ||
                headStr.Contains("mso-application progid=\"Excel.Sheet\"", StringComparison.OrdinalIgnoreCase) ||
                headStr.Contains("<Workbook", StringComparison.OrdinalIgnoreCase))
            {
                return ReadSpreadsheetXml(path);
            }

            // 3) Fallback: parse as delimited text (CSV/TSV/; or |)
            return ReadDelimitedText(path);
        }

        public static void ExportResults(string path, IEnumerable<UserDto> results)
        {
            using var wb = new XLWorkbook();

            void AddSheet(string name, IEnumerable<UserDto> data)
            {
                var ws = wb.Worksheets.Add(name);
                ws.Cell(1, 1).Value = "SamAccountName";
                ws.Cell(1, 2).Value = "DisplayName";
                ws.Cell(1, 3).Value = "Email";
                ws.Cell(1, 4).Value = "Enabled";

                int r = 2;
                foreach (var u in data)
                {
                    ws.Cell(r, 1).Value = u.SamAccountName;
                    ws.Cell(r, 2).Value = u.DisplayName;
                    ws.Cell(r, 3).Value = u.Email;
                    ws.Cell(r, 4).Value = u.Enabled.HasValue ? (u.Enabled.Value ? "Yes" : "No") : "";
                    r++;
                }
                ws.Columns().AdjustToContents();
            }

            AddSheet("Enabled", results.Where(x => x.Category == "Enabled"));
            AddSheet("Disabled", results.Where(x => x.Category == "Disabled"));
            AddSheet("NotFound", results.Where(x => x.Category == "NotFound"));
            AddSheet("External", results.Where(x => x.Category == "External"));

            wb.SaveAs(path);
        }

        // === HELPERS ===

        // True XLS/XLSX (ExcelDataReader)
        private static List<UserDto> ReadRealExcel(string path)
        {
            var users = new List<UserDto>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            var result = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            });

            if (result.Tables.Count == 0) return users;
            var table = result.Tables[0];

            var (idxSam, idxName, idxMail) =
                MapColumns(table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));

            RequireHeaders(idxMail, path);

            foreach (DataRow row in table.Rows)
            {
                string Get(int? i) => i.HasValue ? (row[i.Value]?.ToString() ?? "").Trim() : "";
                var sam = Get(idxSam);
                var email = Get(idxMail);
                if (string.IsNullOrWhiteSpace(sam) && string.IsNullOrWhiteSpace(email))
                    continue;

                users.Add(new UserDto
                {
                    SamAccountName = sam,
                    DisplayName = Get(idxName),
                    Email = email
                });
            }
            return users;
        }

        private static List<UserDto> ReadSpreadsheetXml(string path)
        {
            var users = new List<UserDto>();
            var doc = XDocument.Load(path);
            XNamespace ss = "urn:schemas-microsoft-com:office:spreadsheet";

            var table = doc.Descendants(ss + "Worksheet")
                           .Descendants(ss + "Table")
                           .FirstOrDefault();
            if (table == null) return users;

            List<List<string>> rows = [];
            foreach (var row in table.Elements(ss + "Row"))
            {
                var cells = new List<string>();
                int col = 0;
                foreach (var cell in row.Elements(ss + "Cell"))
                {
                    int index = (int?)cell.Attribute(ss + "Index") ?? (col + 1);
                    while (col + 1 < index) { cells.Add(string.Empty); col++; }

                    string val = cell.Element(ss + "Data")?.Value?.Trim() ?? string.Empty;
                    cells.Add(val);
                    col++;
                }
                rows.Add(cells);
            }

            if (rows.Count == 0) return users;

            var header = rows[0];
            var (idxSam, idxName, idxMail) = MapColumns(header);
            RequireHeaders(idxMail, path);

            foreach (var r in rows.Skip(1))
            {
                string Get(int? i) => (i.HasValue && i.Value < r.Count) ? r[i.Value] : "";
                var sam = Get(idxSam);
                var email = Get(idxMail);
                if (string.IsNullOrWhiteSpace(sam) && string.IsNullOrWhiteSpace(email))
                    continue;

                users.Add(new UserDto
                {
                    SamAccountName = sam,
                    DisplayName = Get(idxName),
                    Email = email
                });
            }
            return users;
        }

        private static List<UserDto> ReadDelimitedText(string path)
        {
            var users = new List<UserDto>();
            var lines = File.ReadAllLines(path);
            if (lines.Length < 2) return users;

            char[] candidates = ['\t', ',', ';', '|'];
            char delimiter = candidates.OrderByDescending(d => lines[0].Count(ch => ch == d)).First();

            var header = lines[0].Split(delimiter).Select(h => h.Trim()).ToList();
            var (idxSam, idxName, idxMail) = MapColumns(header);
            RequireHeaders(idxMail, path);

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var parts = lines[i].Split(delimiter).Select(p => p.Trim()).ToList();

                string Get(int? idx) => (idx.HasValue && idx.Value < parts.Count) ? parts[idx.Value] : "";
                var sam = Get(idxSam);
                var email = Get(idxMail);
                if (string.IsNullOrWhiteSpace(sam) && string.IsNullOrWhiteSpace(email))
                    continue;

                users.Add(new UserDto
                {
                    SamAccountName = sam,
                    DisplayName = Get(idxName),
                    Email = email
                });
            }
            return users;
        }

        private static (int? idxSam, int? idxName, int? idxMail) MapColumns(IEnumerable<string> headers)
        {
            var list = headers.Select((h, i) => new { h, i })
                              .ToDictionary(x => x.h.Trim(), x => x.i, StringComparer.OrdinalIgnoreCase);

            int? Col(params string[] names)
            {
                foreach (var n in names)
                    if (list.TryGetValue(n, out var idx)) return idx;
                return null;
            }

            var idxSam = Col("SamAccountName", "sAMAccountName", "Internal ID", "InternalID", "Username", "User", "ID");
            var idxName = Col("Name", "DisplayName", "FullName");
            var idxMail = Col("EmailAddress", "Email", "Mail", "E-mail", "UPN", "UserPrincipalName");

            return (idxSam, idxName, idxMail);
        }

        // Require EmailAddress (and optionally SAM)
        private static void RequireHeaders(int? idxMail, string sourcePath)
        {
            if (idxMail is null)
                throw new InvalidDataException(
                    $"The file \"{Path.GetFileName(sourcePath)}\" must contain an 'EmailAddress' header (synonyms accepted).");
        }
    }
}
