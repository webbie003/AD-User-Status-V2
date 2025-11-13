using AdUserStatus.Services;

namespace AdUserStatus
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            try
            {
                Application.Run(new MainForm());
            }
            finally
            {
                // Clean up extracted help files on application exit
                HelpForm.CleanupExtractedHelp();
            }
        }
    }
}