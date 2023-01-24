using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransliteratorWPF_Version.Models;
using TransliteratorWPF_Version.Properties;
using TransliteratorWPF_Version.Services;
using TransliteratorWPF_Version.Views;

namespace TransliteratorWPF_Version.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly LoggerService loggerService;
        private readonly LiveTransliterator liveTransliterator;
        private readonly SettingsService settingsService;

        [ObservableProperty]
        private bool shouldPlayToggleSound;

        [ObservableProperty]
        private bool shouldStartMinimized;

        [ObservableProperty]
        private bool shouldRunAtStartup;

        [ObservableProperty]
        private bool isShouldRunAtStartupCheckBoxEnabled;

        [ObservableProperty]
        private string toggleTranslitShortcut;

        [ObservableProperty]
        private HotKey toggleHotKey;

        [ObservableProperty]
        private bool isStateOverlayEnabled;

        [ObservableProperty]
        private bool isTranslitEnabledOnStartup;

        [ObservableProperty]
        private bool isAltShiftGlobalShortcutDisabled;

        [ObservableProperty]
        private bool isDisplayOfBufferCharactersEnabled;

        [ObservableProperty]
        private string showcaseText;

        public SettingsViewModel()
        {
            loggerService = LoggerService.GetInstance();
            liveTransliterator = LiveTransliterator.GetInstance();
            settingsService = new();       
            InitializePropertiesFromSettings();
            IsShouldRunAtStartupCheckBoxEnabled = App.IsAdministrator();
        }

        private void InitializePropertiesFromSettings()
        {
            settingsService.Load();
            ShouldPlayToggleSound = settingsService.ShouldPlayToggleSound;
            ShouldStartMinimized = settingsService.ShouldStartMinimized;
            ShouldRunAtStartup = settingsService.ShouldRunAtStartup;        
            IsStateOverlayEnabled = settingsService.IsStateOverlayEnabled;
            IsTranslitEnabledOnStartup = settingsService.IsTranslitEnabledOnStartup;
            IsAltShiftGlobalShortcutDisabled = settingsService.IsAltShiftGlobalShortcutDisabled;
            IsDisplayOfBufferCharactersEnabled = settingsService.IsDisplayOfBufferCharactersEnabled;
            ToggleHotKey = settingsService.ToggleHotKey;
        }

        partial void OnShouldPlayToggleSoundChanged(bool value)
        {
            Settings.Default.PlaySoundOnTranslitToggle = value;
        }

        public void SaveAllProp()
        {
            settingsService.ShouldPlayToggleSound = ShouldPlayToggleSound;
            settingsService.ShouldStartMinimized = ShouldStartMinimized;
            settingsService.ShouldRunAtStartup = ShouldRunAtStartup;
            settingsService.IsStateOverlayEnabled = IsStateOverlayEnabled;
            settingsService.IsTranslitEnabledOnStartup = IsTranslitEnabledOnStartup;
            settingsService.IsAltShiftGlobalShortcutDisabled = IsAltShiftGlobalShortcutDisabled;
            settingsService.IsDisplayOfBufferCharactersEnabled = IsDisplayOfBufferCharactersEnabled;
            settingsService.ToggleHotKey = ToggleHotKey;
            settingsService.Save();
        }

        partial void OnToggleHotKeyChanged(HotKey value)
        {
            // TODO: Set ToggleTranslitShortcut in LiveTransliterator
        }

        partial void OnIsDisplayOfBufferCharactersEnabledChanged(bool value)
        {
            liveTransliterator.displayCombos = value;

            // TODO: Do we need this?
            //if (debugWindow != null && debugWindow.underTestByWinDriverCheckBox.IsChecked == true)
            //{
            //    return;
            //}
            //else
            //{
            ShowBufferCharacterMode();
            //}
        }

        // TODO: Rewrite this method
        private void ShowBufferCharacterMode()
        {
            //// switch translit table to accentsTable
            //// warning: hardcoded

            //const string accentsTable = "tableAccents.json";
            //string previousTable = liveTransliterator.ukrTranslit.replacement_map_filename;

            //liveTransliterator.ukrTranslit.SetReplacementMapFromJson($"{accentsTable}");

            //// gotta make sure transliterator is enabled. Should reference .keyLogger.state, not liveTranslit.state
            //liveTransliterator.keyLogger.State = true;

            //DOCshowcaseTxtBox.IsEnabled = true;
            //DOCshowcaseTxtBox.Focus();
            //DOCshowcaseTxtBox.Clear();

            //// warning: hardcoded
            //string testString = "`a`o`i`u";

            //await liveTransliterator.WriteInjected(testString);
            //DOCshowcaseTxtBox.IsEnabled = false;

            //liveTransliterator.ukrTranslit.SetReplacementMapFromJson($"{previousTable}");
        }

        [RelayCommand]
        private void OpenTranslitTablesWindow()
        {
            // TODO: Rewrite to NavigateToTranslitTablesPage or prevent the creation of multiple windows
            TranslitTablesWindow translitTables = new TranslitTablesWindow();
            translitTables.Show();
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
        partial void OnIsShouldRunAtStartupCheckBoxEnabledChanged(bool value)
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
