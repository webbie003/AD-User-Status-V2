using Microsoft.Web.WebView2.WinForms;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AdUserStatus.Services
{
    public class HelpForm : Form
    {
        private readonly WebView2 _web;
        private readonly string _topic;

        // Shared temp folder for all help content
        private static readonly string HelpRootFolder =
            Path.Combine(Path.GetTempPath(), "ADUserStatusHelp");

        // Default topic
        public HelpForm() : this("index.html")
        {
        }

        // Topic-aware (used by HelpAwareForm / F1)
        public HelpForm(string topic)
        {
            _topic = string.IsNullOrWhiteSpace(topic) ? "index.html" : topic;

            Text = "Help & Support";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1024, 800);
            Padding = Padding.Empty;
            Margin = Padding.Empty;

            _web = new WebView2 { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Controls.Add(_web);

            Load += async (_, __) =>
            {
                await _web.EnsureCoreWebView2Async();

                // For normal navigations (same window)
                _web.CoreWebView2.NavigationStarting += (s, e) =>
                {
                    var uri = e.Uri;

                    if (uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Cancel = true;
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = uri,
                                UseShellExecute = true
                            });
                        }
                        catch { }
                    }
                };

                // For links that try to open a new window (target="_blank", window.open, etc.)
                _web.CoreWebView2.NewWindowRequested += (s, e) =>
                {
                    var uri = e.Uri;

                    if (!string.IsNullOrEmpty(uri) &&
                        (uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                         uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                    {
                        e.Handled = true; // prevent WebView2 from creating a new window
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = uri,
                                UseShellExecute = true
                            });
                        }
                        catch { }
                    }
                };

                _web.CoreWebView2.Settings.IsScriptEnabled = false;
                _web.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                _web.CoreWebView2.Settings.IsWebMessageEnabled = false;

                string helpRoot = ExtractHelpFilesFromResources();

                string htmlPath = Path.Combine(helpRoot, _topic);
                if (!File.Exists(htmlPath))
                    htmlPath = Path.Combine(helpRoot, "index.html");

                _web.Source = new Uri(htmlPath);
            };
        }

        private static string ExtractHelpFilesFromResources()
        {
            var asm = Assembly.GetExecutingAssembly();

            Directory.CreateDirectory(HelpRootFolder);

            foreach (string resName in asm.GetManifestResourceNames()
                                          .Where(n => n.Contains(".Help.", StringComparison.OrdinalIgnoreCase)))
            {
                using Stream? s = asm.GetManifestResourceStream(resName);
                if (s == null) continue;

                // Strip up to and including ".Help."
                int idx = resName.IndexOf(".Help.", StringComparison.OrdinalIgnoreCase);
                string suffix = resName.Substring(idx + ".Help.".Length);   // e.g. "Images.binoculars.png" or "index.html"

                string[] parts = suffix.Split('.');

                string fileName;
                string? relDir = null;

                if (parts.Length == 1)
                {
                    fileName = parts[0];
                }
                else if (parts.Length == 2)
                {
                    // "index", "html" -> "index.html"
                    fileName = string.Join('.', parts);
                }
                else
                {
                    // "Images", "binoculars", "png" -> dir: "Images", file: "binoculars.png"
                    fileName = string.Join('.', parts[^2], parts[^1]);
                    if (parts.Length > 2)
                    {
                        relDir = Path.Combine(parts[..^2]);
                    }
                }

                string dir = HelpRootFolder;
                if (!string.IsNullOrEmpty(relDir))
                {
                    dir = Path.Combine(HelpRootFolder, relDir);
                    Directory.CreateDirectory(dir);
                }

                string filePath = Path.Combine(dir, fileName);

                // Always overwrite with the latest embedded content
                using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                s.CopyTo(fs);
            }

            return HelpRootFolder;
        }
        
        public static void CleanupExtractedHelp()
        {
            try
            {
                if (Directory.Exists(HelpRootFolder))
                {
                    Directory.Delete(HelpRootFolder, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup errors (files in use, permissions, etc.)
            }
        }
    }
}
