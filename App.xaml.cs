using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Principal;
using System.Windows;
using TransliteratorWPF_Version.Helpers;

using TransliteratorWPF_Version.Services;

namespace TransliteratorWPF_Version
{
    public partial class App : Application
    {
        public static string AppName = "Transliterator";

        public new static App Current => (App)Application.Current;

        public App()
        {
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        public static string GetFullPath(string RelativePath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RelativePath);
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