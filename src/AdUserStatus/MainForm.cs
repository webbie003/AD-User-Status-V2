using AdUserStatus.Models;
using AdUserStatus.Services;
using System.ComponentModel; // BindingList

namespace AdUserStatus
{
    public partial class MainForm : HelpAwareForm
    {
        // ===== Data for the four grids =====
        private readonly BindingList<UserDto> _enabled = [];
        private readonly BindingList<UserDto> _disabled = [];
        private readonly BindingList<UserDto> _notFound = [];
        private readonly BindingList<UserDto> _external = [];

        // ===== Callout =====
        private bool _readyCalloutShown;

        // ===== Connection state & status UI =====
        private bool _useLdaps = false;       // chosen after startup probe
        private string? _autoDc;              // DC discovered/tested

        private StatusStrip _statusStrip = null!;
        private ToolStripStatusLabel _connStatus = null!;
        private CancellationTokenSource? _cts;

        public MainForm()
        {
            InitializeComponent();
            //Theme.Apply(this);
            MinimumSize = new Size(600, 480);       // prevent window from being shrunk too small
            buttonHelp.Click += ButtonHelp_Click;   // Help
            WireUpUi();
            this.Shown += async (_, __) => await AutoDetectDirectoryAsync();
        }

        private static void AddGridContextMenu(DataGridView grid)
        {
            var menu = new ContextMenuStrip();

            var copyEmail = new ToolStripMenuItem("Copy Email", null, (_, __) =>
            {
                if (grid.CurrentRow?.DataBoundItem is UserDto u && !string.IsNullOrWhiteSpace(u.Email))
                    Clipboard.SetText(u.Email);
            });

            var copyRow = new ToolStripMenuItem("Copy Row", null, (_, __) =>
            {
                if (grid.CurrentRow?.DataBoundItem is UserDto u)
                    Clipboard.SetText($"{u.DisplayName}\t{u.Email}\t{u.Enabled}\t{u.Category}");
            });

            menu.Items.AddRange([copyEmail, copyRow]);
            grid.ContextMenuStrip = menu;
        }

        private static void SetupGrid(DataGridView grid)
        {
            grid.DataBindingComplete += (_, __) =>
            {
                // Hide columns from grid, these will be present on the export.
                if (grid.Columns.Contains("SamAccountName"))
                    grid.Columns["SamAccountName"].Visible = false;

                if (grid.Columns.Contains("Category"))
                    grid.Columns["Category"].Visible = false;

                // Rename "Enabled" column to "Status"
                if (grid.Columns.Contains("Enabled"))
                    grid.Columns["Enabled"].HeaderText = "Status";

                // Auto-fit column widths to cell values + headers
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

                // Optional: adjust row heights to fit text (multi-line display names etc.)
                grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            };

            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    var col = grid.Columns[e.ColumnIndex];

                    // Target the "Enabled" column
                    if (col.Name == "Enabled")
                    {
                        // Handle bool? values
                        if (e.Value is bool val)
                        {
                            e.Value = val ? "Enabled" : "Disabled";
                            e.FormattingApplied = true;
                        }
                        else if (e.Value == null)
                        {
                            e.Value = "";
                            e.FormattingApplied = true;
                        }
                    }
                }
            };

            // Presentation polish
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void WireUpUi()
        {
            // Bind grids
            gridEnabled.AutoGenerateColumns = true;
            gridDisabled.AutoGenerateColumns = true;
            gridNotFound.AutoGenerateColumns = true;
            gridExternal.AutoGenerateColumns = true;

            gridEnabled.DataSource = _enabled;
            gridDisabled.DataSource = _disabled;
            gridNotFound.DataSource = _notFound;
            gridExternal.DataSource = _external;

            // Grid context menus
            AddGridContextMenu(gridEnabled);
            AddGridContextMenu(gridDisabled);
            AddGridContextMenu(gridNotFound);
            AddGridContextMenu(gridExternal);

            // Column sizing / polish
            SetupGrid(gridEnabled);
            SetupGrid(gridDisabled);
            SetupGrid(gridNotFound);
            SetupGrid(gridExternal);

            AttachEmptyStateCue(gridEnabled, "Drag & drop a file to begin");
            AttachEmptyStateCue(gridDisabled, "Drag & drop a file to begin");
            AttachEmptyStateCue(gridNotFound, "Drag & drop a file to begin");
            AttachEmptyStateCue(gridExternal, "Drag & drop a file to begin");

            // Buttons
            button1.Click += Button1_Browse_Click;   // Browse
            button2.Click += Button2_Process_Click;  // Check User / Process
            button3.Click += Button3_Export_Click;   // Export Results
            button4.Click += (_, __) => Close();     // Exit

            // Status bar
            _statusStrip = new StatusStrip { SizingGrip = true, Dock = DockStyle.Bottom };
            _connStatus = new ToolStripStatusLabel("Checking directory…");
            _statusStrip.Items.Add(_connStatus);
            Controls.Add(_statusStrip);

            // Disable 'Check User' & 'Export' until conditions are met
            button2.Enabled = false; // LDAP/LDAPS connection established enables this.
            button3.Enabled = false; // Enabled when data is present.

            // Drag & drop the input file
            AllowDrop = true;
            this.DragEnter += (_, e) =>
            {
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true) e.Effect = DragDropEffects.Copy;
            };
            this.DragDrop += (_, e) =>
            {
                var files = (string[])(e.Data!.GetData(DataFormats.FileDrop)!);
                if (files.Length > 0) textBox1.Text = files[0];
            };
        }

        // Startup: Auto-detect LDAP(S)
        private async Task AutoDetectDirectoryAsync()
        {
            try
            {
                _connStatus.Text = "Checking LDAP/LDAPS connectivity…";
                _connStatus.ForeColor = SystemColors.ControlText;

                _autoDc = DcDiscovery.GetPreferredDomainController();

                var ldapsOk = await TestBindAsync(_autoDc, useLdaps: true);
                if (ldapsOk)
                {
                    _useLdaps = true;
                    SetConnStatus($"Connected via LDAPS (Secure): {_autoDc}:636", ConnKind.LdapsOk);
                    button2.Enabled = true;
                    return;
                }

                var ldapOk = await TestBindAsync(_autoDc, useLdaps: false);
                if (ldapOk)
                {
                    _useLdaps = false;
                    SetConnStatus($"Connected via LDAP (Insecure): {_autoDc}:389", ConnKind.LdapOk);
                    button2.Enabled = true;
                    ShowReadyCallout();
                }
                else
                {
                    _useLdaps = false;
                    SetConnStatus($"No LDAP/LDAPS connectivity to {_autoDc}", ConnKind.Error);
                    button2.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                _useLdaps = false;
                SetConnStatus($"Directory check failed: {ex.Message}", ConnKind.Error);
                button2.Enabled = false;
            }
        }

        private static async Task<bool> TestBindAsync(string host, bool useLdaps, int timeoutMs = 5000)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var port = useLdaps ? 636 : 389;
                    var id = new System.DirectoryServices.Protocols.LdapDirectoryIdentifier(host, port, false, false);
                    using var conn = new System.DirectoryServices.Protocols.LdapConnection(id)
                    {
                        Timeout = TimeSpan.FromMilliseconds(timeoutMs),
                        AuthType = System.DirectoryServices.Protocols.AuthType.Negotiate
                    };
                    conn.SessionOptions.ProtocolVersion = 3;
                    conn.SessionOptions.SecureSocketLayer = useLdaps;
                    conn.Bind();
                    return true;
                }
                catch { return false; }
            });
        }

        private enum ConnKind { LdapsOk, LdapOk, Error }
        private void SetConnStatus(string message, ConnKind kind)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => SetConnStatus(message, kind))); return; }

            _connStatus.Text = message;
            _connStatus.ForeColor = kind switch
            {
                ConnKind.LdapsOk => Color.ForestGreen,
                ConnKind.LdapOk => Color.DarkOrange,
                _ => Color.IndianRed
            };
            _statusStrip.Refresh(); // force paint now
        }

        // Reusable fonts
        private static readonly Font GridCellFont = new("Segoe UI", 10f, FontStyle.Regular);
        private static readonly Font GridHeaderFont = new("Segoe UI", 10f, FontStyle.Bold);

        private void Form1_Load(object sender, EventArgs e)
        {
            SetGridStyle(gridEnabled);
            SetGridStyle(gridDisabled);
            SetGridStyle(gridNotFound);
            SetGridStyle(gridExternal);
        }

        private static void SetGridStyle(DataGridView grid)
        {
            grid.DefaultCellStyle.Font = GridCellFont;
            grid.ColumnHeadersDefaultCellStyle.Font = GridHeaderFont;
            grid.RowHeadersDefaultCellStyle.Font = GridCellFont;

            EnableDoubleBuffer(grid, true); // optional for smoother redraw

            void ApplyColumnDefaults()
            {
                const int MIN = 210;

                // compute preferred widths first
                grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

                foreach (DataGridViewColumn col in grid.Columns)
                {
                    int preferred = col.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
                    int width = Math.Max(preferred, MIN);

                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // lock
                    col.Width = width;                                      // clamp
                    col.MinimumWidth = MIN;                                 // future safety
                }
            }

            // (Re)attach handlers
            grid.DataBindingComplete -= OnDataBindingComplete;
            grid.DataBindingComplete += OnDataBindingComplete;
            grid.ColumnAdded -= OnColumnAdded;
            grid.ColumnAdded += OnColumnAdded;

            void OnDataBindingComplete(object? s, DataGridViewBindingCompleteEventArgs e) => ApplyColumnDefaults();
            void OnColumnAdded(object? s, DataGridViewColumnEventArgs e) => ApplyColumnDefaults();

            // Apply once now to cover columns that already exist
            ApplyColumnDefaults();
        }

        private static void EnableDoubleBuffer(DataGridView grid, bool enable = true)
        {
            var pi = typeof(DataGridView).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pi?.SetValue(grid, enable, null);
        }

        private void Button1_Browse_Click(object? sender, EventArgs e)
        {
            ResetReadyCalloutState();

            var previous = textBox1.Text;
            textBox1.Clear();

            using var ofd = new OpenFileDialog
            {
                Filter = "Excel/Spreadsheet Files (*.xlsx;*.xls;*.xml;*.csv;*.tsv;*.txt)|*.xlsx;*.xls;*.xml;*.csv;*.tsv;*.txt|All Files (*.*)|*.*",
                Title = "Select input file"
            };

            var result = ofd.ShowDialog(this);

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
            {
                textBox1.Text = ofd.FileName;

                // Ensure form is active and schedule the callout after focus settles
                this.Activate();
                BeginInvoke(new Action(() => ShowReadyCallout(force: true)));
            }
            else
            {
                textBox1.Text = previous;

                //if (!string.IsNullOrWhiteSpace(previous) && button2.Enabled)
                //{
                //    this.Activate();
                //    BeginInvoke(new Action(() => ShowReadyCallout(force: true)));
                //}
            }
        }


        private async void Button2_Process_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show(this, "Please select an Excel file first.", "Missing input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            // Reset UI
            _enabled.Clear(); _disabled.Clear(); _notFound.Clear(); _external.Clear();
            UpdateTabCounts();
            button3.Enabled = false;
            SetRunning(true);
            ResetReadyCalloutState();

            try
            {
                var users = ExcelService.ReadUsers(textBox1.Text);
                if (users.Count == 0)
                {
                    MessageBox.Show(this,
                        "No users found. The spreadsheet MUST include EmailAddress (and optionally 'SamAccountName') headers.",
                        "No data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var dc = _autoDc ?? DcDiscovery.GetPreferredDomainController();

                _cts?.Dispose();
                _cts = new CancellationTokenSource();
                var token = _cts.Token;

                using var dlg = new ProcessingDialog("Querying Active Directory…");
                var progress = new Progress<(int current, int total)>(p => dlg.SetProgress(p.current, p.total));

                // signal set when user clicks Cancel (so we can return immediately)
                var cancelledTcs = new TaskCompletionSource();

                dlg.CancelRequested += (_, __) =>
                {
                    _cts.Cancel();
                    dlg.SetMessage("Cancelling…");
                    cancelledTcs.TrySetResult();
                    if (dlg.InvokeRequired) dlg.BeginInvoke(new Action(dlg.Close));
                    else dlg.Close();
                };

                var processingTask = Task.Run(() =>
                    ProcessUsers(dc, _useLdaps, users, token, progress), token);

                // Show modal dialog (closes automatically at 100% or via Cancel)
                dlg.ShowDialog(this);

                // Whichever finishes first: processing or user cancelled
                var first = await Task.WhenAny(processingTask, cancelledTcs.Task);

                if (first == processingTask)
                {
                    var results = await processingTask;

                    foreach (var u in results.Enabled) _enabled.Add(u);
                    foreach (var u in results.Disabled) _disabled.Add(u);
                    foreach (var u in results.NotFound) _notFound.Add(u);
                    foreach (var u in results.External) _external.Add(u);

                    UpdateTabCounts();
                    button3.Enabled = _enabled.Any() || _disabled.Any() || _notFound.Any() || _external.Any();

                    tabResults.SelectedTab = tabEnabled;
                }
                else
                {
                    _ = processingTask.ContinueWith(t => { var _ = t.Exception; }, TaskScheduler.Default);

                    MessageBox.Show(this, "Operation cancelled.", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Processing error:\n\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetRunning(false);
                _cts?.Dispose();
                _cts = null;
            }
        }

        private void Button3_Export_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Save results",
                FileName = "AD_User_Status.xlsx"
            };
            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            var all = new List<UserDto>();
            all.AddRange(_enabled.Select(u => { u.Category = "Enabled"; return u; }));
            all.AddRange(_disabled.Select(u => { u.Category = "Disabled"; return u; }));
            all.AddRange(_notFound.Select(u => { u.Category = "NotFound"; return u; }));
            all.AddRange(_external.Select(u => { u.Category = "External"; return u; }));

            try
            {
                ExcelService.ExportResults(sfd.FileName, all);
                MessageBox.Show(this, "Export complete.", "Done",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Export failed:\n\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonHelp_Click(object? sender, EventArgs e)
        {
            var hf = new HelpForm
            {
                StartPosition = FormStartPosition.CenterParent,
                Icon = this.Icon
            };
            hf.Show(this);
        }

        private static void AttachEmptyStateCue(DataGridView grid, string text)
        {
            // repaint when data changes
            grid.DataBindingComplete += (_, __) => grid.Invalidate();
            grid.RowsAdded += (_, __) => grid.Invalidate();
            grid.RowsRemoved += (_, __) => grid.Invalidate();

            // draw the centered message when empty
            grid.Paint += (s, e) =>
            {
                int count = grid.Rows.Count;
                if (count > 0) return;

                var rect = grid.DisplayRectangle;
                rect.Inflate(-24, -24);

                using var f = new Font(grid.Font.FontFamily, grid.Font.Size + 4, FontStyle.Bold | FontStyle.Italic);

                TextRenderer.DrawText(
                    e.Graphics,
                    text.Replace("&", "&&"),  // <-- escape ampersand for literal display
                    f,
                    rect,
                    SystemColors.GrayText,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            };
        }

        // Core processing
        private static (List<UserDto> Enabled, List<UserDto> Disabled, List<UserDto> NotFound, List<UserDto> External)
            ProcessUsers(string dc, bool useLdaps, List<UserDto> input, CancellationToken token,
                         IProgress<(int current, int total)>? progress = null)
        {
            var enabled = new List<UserDto>();
            var disabled = new List<UserDto>();
            var notFound = new List<UserDto>();
            var external = new List<UserDto>();

            using var ldap = new LdapService(dc, useLdaps);

            // Discover internal domains once per run
            var internalDomains = ldap.GetInternalDomains();

            for (int i = 0; i < input.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                var row = input[i];
                try
                {
                    var email = (row.Email ?? "").Trim();
                    var samIn = (row.SamAccountName ?? "").Trim();
                    string? domain = email.Contains('@') ? email.Split('@')[1].Trim().ToLowerInvariant() : null;

                    if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(samIn))
                    {
                        // nothing to query
                        notFound.Add(new UserDto
                        {
                            SamAccountName = row.SamAccountName ?? string.Empty,
                            DisplayName = row.DisplayName ?? string.Empty,
                            Email = row.Email ?? string.Empty,
                            Enabled = null,
                            Category = "NotFound"
                        });
                    }
                    else
                    {
                        // 1) Try by email (matches mail/UPN/proxyAddresses); 2) fallback to sAMAccountName
                        (bool found, bool? isEnabled, string? displayName, string? sam, string? upn) res = (false, null, null, null, null);

                        if (!string.IsNullOrWhiteSpace(email))
                            res = ldap.FindByEmail(email);

                        if (!res.found && !string.IsNullOrWhiteSpace(samIn))
                            res = ldap.FindBySamAccountName(samIn);

                        if (res.found)
                        {
                            var dto = new UserDto
                            {
                                SamAccountName = res.sam ?? samIn,
                                DisplayName = res.displayName ?? row.DisplayName,
                                Email = string.IsNullOrWhiteSpace(email) ? (res.upn ?? row.Email) : email,
                                Enabled = res.isEnabled,
                                Category = res.isEnabled == true ? "Enabled" :
                                                 res.isEnabled == false ? "Disabled" : "NotFound"
                            };

                            if (dto.Category == "Enabled") enabled.Add(dto);
                            else if (dto.Category == "Disabled") disabled.Add(dto);
                            else notFound.Add(dto);
                        }
                        else
                        {
                            // Not found in AD: classify External vs NotFound
                            var dto = new UserDto
                            {
                                SamAccountName = samIn,
                                DisplayName = row.DisplayName,
                                Email = email,
                                Enabled = null
                            };

                            bool isInternal = !string.IsNullOrWhiteSpace(domain) && internalDomains.Contains(domain);
                            dto.Category = isInternal ? "NotFound" : "External";

                            if (dto.Category == "External") external.Add(dto);
                            else notFound.Add(dto);
                        }
                    }
                }
                catch (OperationCanceledException) { throw; }
                catch
                {
                    notFound.Add(new UserDto
                    {
                        SamAccountName = row.SamAccountName,
                        DisplayName = row.DisplayName,
                        Email = row.Email,
                        Enabled = null,
                        Category = "NotFound"
                    });
                }

                // ✅ This is where you update the live progress
                progress?.Report((i + 1, input.Count));
            }

            progress?.Report((input.Count, input.Count)); // final update (100%)
            return (enabled, disabled, notFound, external);
        }

        // Utilities
        private void SetRunning(bool running)
        {
            button1.Enabled = !running; // Browse
            button2.Enabled = !running; // Process
            button3.Enabled = !running; // Export
            textBox1.Enabled = !running;
            UseWaitCursor = running;
        }

        private void UpdateTabCounts()
        {
            tabEnabled.Text = $"Enabled ({_enabled.Count})";
            tabDisabled.Text = $"Disabled ({_disabled.Count})";
            tabNotFound.Text = $"NotFound ({_notFound.Count})";
            tabExternal.Text = $"External ({_external.Count})";
        }

        private readonly ToolTip _readyTip = new()
        {
            IsBalloon = true,
            InitialDelay = 100,
            ReshowDelay = 500,
            AutoPopDelay = 4000,
            ToolTipIcon = ToolTipIcon.Info
        };

        private void ShowReadyCallout(bool force = false)
        {
            // Must run on UI thread (AutoDetectDirectoryAsync is async)
            if (InvokeRequired) { BeginInvoke(new Action(() => ShowReadyCallout(force))); return; }

            // Guard: only when both conditions are met
            if (string.IsNullOrWhiteSpace(textBox1.Text) || !button2.Enabled) return;

            // Avoid repeating unless forced
            if (_readyCalloutShown && !force) return;
            _readyCalloutShown = true;

            // Configure and show the balloon
            _readyTip.IsBalloon = true;
            _readyTip.ToolTipIcon = ToolTipIcon.None;
            _readyTip.ToolTipTitle = "Ready";
            _readyTip.UseFading = true;
            _readyTip.UseAnimation = true;
            _readyTip.Hide(this);
            _readyTip.Hide(button2);

            int x = button2.Width / 2;
            int y = -button2.Height - 50; // above the button, adjust as you like
            _readyTip.Show("Click here to begin...", button2, x, y, 3000);

        }

        private void ResetReadyCalloutState()
        {
            _readyCalloutShown = false;
            _readyTip.Hide(button2);
        }
    }
}
