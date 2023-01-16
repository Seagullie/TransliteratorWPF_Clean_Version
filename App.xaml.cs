using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Transliterator;
using TransliteratorWPF_Version.Properties;

namespace TransliteratorBackend
{
    public partial class App : Application
    {
        public static string BaseDir;

        public string appName = "Transliterator";
        public LiveTransliterator liveTranslit;

        public App() : base()
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
            string[] tables = GlobalUtilities.getAllFilesInFolder(@"Resources/translitTables");

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