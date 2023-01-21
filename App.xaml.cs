using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows;
using TransliteratorWPF_Version.Helpers;
using TransliteratorWPF_Version.Properties;
using TransliteratorWPF_Version.Services;

namespace TransliteratorWPF_Version
{
    public partial class App : Application
    {
        public static string BaseDir;

        public string appName = "Transliterator";
        public LiveTransliterator liveTranslit;

        public new static App Current => (App)Application.Current;

        public App()
        {
            string PathToTransliteratorAsSettingVar = Settings.Default.ProgramFolder;

            if (PathToTransliteratorAsSettingVar == "")
            {
                Settings.Default.ProgramFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                BaseDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                Settings.Default.Save();
            }
            else
            {
                BaseDir = PathToTransliteratorAsSettingVar;
            }

            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        public static string GetFullPath(string RelativePath)
        {
            return Path.Combine(BaseDir, RelativePath);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string stackTraceAsString = e.Exception.StackTrace.ToString();

            MessageBox.Show($"Unhandled exception occurred: \n{e.Exception.Message}. Stack Trace:\n{stackTraceAsString}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public string[] getAllTranslitTableNames()
        {
            string[] tables = Utilities.getAllFilesInFolder(@"Resources/TranslitTables");

            return tables;
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}