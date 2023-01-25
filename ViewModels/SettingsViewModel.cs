using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using TransliteratorWPF_Version.Models;
using TransliteratorWPF_Version.Services;
using TransliteratorWPF_Version.Views;

namespace TransliteratorWPF_Version.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly LoggerService loggerService;
        private readonly Main liveTransliterator;
        private readonly SettingsService settingsService;

        [ObservableProperty]
        private bool isToggleSoundOn;

        [ObservableProperty]
        private bool isMinimizedStartEnabled;

        [ObservableProperty]
        private bool isAutoStartEnabled;

        [ObservableProperty]
        private bool isAdministrator;

        [ObservableProperty]
        private string toggleTranslitShortcut;

        [ObservableProperty]
        private HotKey toggleHotKey;

        [ObservableProperty]
        private bool isStateOverlayEnabled;

        [ObservableProperty]
        private bool isTranslitEnabledAtStartup;

        [ObservableProperty]
        private bool isAltShiftGlobalShortcutEnabled;

        [ObservableProperty]
        private bool isBufferInputEnabled;

        [ObservableProperty]
        private string showcaseText;

        public SettingsViewModel()
        {
            // TODO: Dependency injection
            loggerService = LoggerService.GetInstance();
            liveTransliterator = Main.GetInstance();
            settingsService = SettingsService.GetInstance();

            InitializePropertiesFromSettings();
            IsAdministrator = App.IsAdministrator();
        }

        private void InitializePropertiesFromSettings()
        {
            settingsService.Load();
            IsToggleSoundOn = settingsService.IsToggleSoundOn;
            IsMinimizedStartEnabled = settingsService.IsMinimizedStartEnabled;
            IsAutoStartEnabled = settingsService.IsAutoStartEnabled;
            IsStateOverlayEnabled = settingsService.IsStateOverlayEnabled;
            IsTranslitEnabledAtStartup = settingsService.IsTranslitEnabledAtStartup;
            IsAltShiftGlobalShortcutEnabled = settingsService.IsAltShiftGlobalShortcutEnabled;
            IsBufferInputEnabled = settingsService.IsBufferInputEnabled;
            ToggleHotKey = settingsService.ToggleHotKey;
        }

        public void SaveAllProps()
        {
            settingsService.IsToggleSoundOn = IsToggleSoundOn;
            settingsService.IsMinimizedStartEnabled = IsMinimizedStartEnabled;
            settingsService.IsAutoStartEnabled = IsAutoStartEnabled;
            settingsService.IsStateOverlayEnabled = IsStateOverlayEnabled;
            settingsService.IsTranslitEnabledAtStartup = IsTranslitEnabledAtStartup;
            settingsService.IsAltShiftGlobalShortcutEnabled = IsAltShiftGlobalShortcutEnabled;
            settingsService.IsBufferInputEnabled = IsBufferInputEnabled;
            settingsService.ToggleHotKey = ToggleHotKey;
            settingsService.Save();
        }

        partial void OnToggleHotKeyChanged(HotKey value)
        {
            // TODO: Set ToggleTranslitShortcut in LiveTransliterator
        }

        partial void OnIsBufferInputEnabledChanged(bool value)
        {
            liveTransliterator.displayCombos = value;

            // TODO: Add DebugService
            //if (debugWindow != null && debugWindow.underTestByWinDriverCheckBox.IsChecked == true)
            //{
            //    return;
            //}
            //else
            //{
            if (value)
                ShowBufferInputIsEnabled(value);
            else
                ShowBufferInputIsDisabled(value);
            //}
        }

        private async void ShowBufferInputIsEnabled(bool isBufferInputEnabled)
        {
            ShowcaseText = "";

            const string showcaseString = "áóíú";
            
            foreach (char charter in showcaseString)
            {
                ShowcaseText += charter;
                await Task.Delay(300);
            }
        }

        private async void ShowBufferInputIsDisabled(bool isBufferInputEnabled)
        {
            ShowcaseText = "";

            const string showcaseString = "`a`o`i`u";
            const string showcaseString2 = "áóíú";

            for (int i = 0; i < showcaseString.Length; i++)
            {
                ShowcaseText += showcaseString[i];
                await Task.Delay(300);
                if ((i + 1) % 2 == 0)
                {
                    await Task.Delay(150);
                    ShowcaseText = ShowcaseText.Remove(ShowcaseText.Length - 2, 2);
                    ShowcaseText += showcaseString2[i / 2];
                    await Task.Delay(300);
                }                
            }
        }

        [RelayCommand]
        private void OpenTranslitTablesWindow()
        {
            // TODO: Rewrite to NavigateToTranslitTablesPage or prevent the creation of multiple windows
            TranslitTablesWindow translitTables = new TranslitTablesWindow();
            translitTables.Show();
        }

        // should simply save the settings, just like it is done when settings window is closed
        [RelayCommand]
        private void ApplyChanges()
        {
            SaveAllProps();
        }

        [RelayCommand]
        private void OpenEditToggleSoundsWindow()
        {
            // TODO: Rewrite to prevent the creation of multiple windows
            EditToggleSoundsWindow editToggleSoundsWindow = new EditToggleSoundsWindow();
            editToggleSoundsWindow.ShowDialog();
        }

        partial void OnIsStateOverlayEnabledChanged(bool value)
        {
            // TODO: Write StateOverlayService
        }

        // TODO: Write the implementation
        partial void OnIsAutoStartEnabledChanged(bool value)
        {
            //private void launchProgramOnSystemStartupCheckBox_Checked(object sender, RoutedEventArgs e)
            //{
            //    if (!App.IsAdministrator())
            //    {
            //        return;
            //    }

            //    var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            //    RegistryKey key = Registry.LocalMachine.OpenSubKey(path, true);

            //    string pathToExecutable = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
            //    key.SetValue(app.appName, pathToExecutable);
            //}

            //private void launchProgramOnSystemStartupCheckBox_Unchecked(object sender, RoutedEventArgs e)
            //{
            //    if (!App.IsAdministrator())
            //    {
            //        return;
            //    }

            //    var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            //    RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
            //    key.DeleteValue(app.appName, false);
            //}
        }
    }
}