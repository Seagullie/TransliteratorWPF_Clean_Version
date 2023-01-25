using Microsoft.Win32;

namespace TransliteratorWPF_Version.Services
{
    public class RegistryService
    {
        private static RegistryService _instance;

        private RegistryService()
        {
        }

        public static RegistryService GetInstance()
        {
            _instance ??= new RegistryService();
            return _instance;
        }

        public bool AddCurrentProgramToAutoStart()
        {
            if (!App.IsAdministrator())
            {
                return false;
            }

            var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(path, true);

            string pathToExecutable = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
            key.SetValue(App.appName, pathToExecutable);

            return true;
        }

        public bool RemoveCurrentProgramFromAutoStart()
        {
            if (!App.IsAdministrator())
            {
                return false;
            }

            var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
            key.DeleteValue(App.appName, false);

            return true;
        }
    }
}