using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading;
using System.Threading.Tasks;
using TransliteratorWPF_Version.Models;
using TransliteratorWPF_Version.Services;
using TransliteratorWPF_Version.Views;

namespace TransliteratorWPF_Version.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly Main liveTransliterator;
        private readonly SettingsService settingsService;

        [ObservableProperty]
        private bool isToggleSoundOn;

        [ObservableProperty]
        private bool isMinimizedStartEnabled;

        [ObservableProperty]
        private bool isAutoStartEnabled;

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
            liveTransliterator = Main.GetInstance();
            settingsService = SettingsService.GetInstance();

            InitializePropertiesFromSettings();
        }

        public void InitializePropertiesFromSettings()
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
            if (ShowcaseBufferInputEnabledModeCommand.IsRunning)
                ShowcaseBufferInputEnabledModeCommand.Cancel();

            if (ShowcaseBufferInputDisabledModeCommand.IsRunning)
                ShowcaseBufferInputDisabledModeCommand.Cancel();

            ShowcaseText = "";

            if (value)
                ShowcaseBufferInputEnabledModeCommand.ExecuteAsync(null);
            else
                ShowcaseBufferInputDisabledModeCommand.ExecuteAsync(null);
            //}
        }

        [RelayCommand]
        private async Task ShowcaseBufferInputEnabledMode(CancellationToken cancellationToken)
        {
            const string showcaseString = "âõíü";

            foreach (char charter in showcaseString)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                ShowcaseText += charter;
                await Task.Delay(300);
            }
        }

        [RelayCommand]
        private async Task ShowcaseBufferInputDisabledMode(CancellationToken cancellationToken)
        {
            const string showcaseString = "^a~o`i\"u";
            const string showcaseString2 = "âõíü";

            for (int i = 0; i < showcaseString.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

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
        private static void OpenTranslitTablesWindow()
        {
            // TODO: Rewrite to NavigateToTranslitTablesPage or prevent the creation of multiple windows
            TranslitTablesWindow translitTables = new();
            translitTables.Show();
        }

        [RelayCommand]
        private void ApplyChanges()
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

        [RelayCommand]
        private static void OpenEditToggleSoundsWindow()
        {
            EditToggleSoundsWindow editToggleSoundsWindow = new();
            editToggleSoundsWindow.ShowDialog();
        }
    }
}