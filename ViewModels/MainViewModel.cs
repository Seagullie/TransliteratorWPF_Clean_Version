using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using TransliteratorWPF_Version.Services;
using TransliteratorWPF_Version.Views;

namespace TransliteratorWPF_Version.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly SettingsService settingsService;
        private readonly TransliteratorService transliteratorService;
        private readonly KeyLoggerService keyLoggerService;

        private NotifyIcon notifyIcon;

        // TODO: Rewrite to bool
        [ObservableProperty]
        private string appState;

        [ObservableProperty]
        private WindowState windowState;

        [ObservableProperty]
        private bool isVisibleInTaskbar = true;

        [ObservableProperty]
        private List<string> translitTables;

        [ObservableProperty]
        private string selectedTranslitTable;

        [ObservableProperty]
        private string toggleAppStateShortcut;

        public MainViewModel()
        {
            // TODO: Dependency injection
            transliteratorService = TransliteratorService.GetInstance();
            settingsService = SettingsService.GetInstance();
            keyLoggerService = KeyLoggerService.GetInstance();

            settingsService.Load();
            settingsService.SettingsSavedEvent += UpdateSettings;

            InitializeWindow();
            InitializeNotifyIcon();
            InitializeStateOverlayWindow();
            InitializeAppState();
            LoadTranslitTables();

            ToggleAppStateShortcut = settingsService.ToggleHotKey.ToString();
        }

        private void UpdateSettings(object sender, EventArgs eventArgs)
        {
            ToggleAppStateShortcut = settingsService.ToggleHotKey.ToString();
        }

        private void InitializeWindow()
        {
            if (settingsService.IsMinimizedStartEnabled)
                WindowState = WindowState.Minimized;

            ThemeManager.Current.ApplicationTheme = settingsService.ApplicationTheme;
            ToggleAppStateShortcut = settingsService.ToggleHotKey.ToString();
        }

        private void InitializeNotifyIcon()
        {
            // TODO: Rewrite using own FileServise
            string str = "TransliteratorWPF_Version.Resources.Images.keyboardIconMarginless.ico";
            Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(str);
            System.Drawing.Icon icon = new(s);

            notifyIcon = new NotifyIcon()
            {
                Icon = icon,
                Visible = true
            };

            // TODO: Fix window state assignement

            notifyIcon.Click +=
                delegate (object sender, EventArgs args)
                {
                    WindowState = WindowState.Normal;
                };
        }

        private void InitializeStateOverlayWindow()
        {
            if (settingsService.IsStateOverlayEnabled)
            {
                StateOverlayWindow stateOveralyWindow = new();
                stateOveralyWindow.Show();
            }
        }

        // TODO: Improve method logic
        private void InitializeAppState()
        {
            SoundPlayerService.IsMuted = true;

            if (keyLoggerService.State)
                AppState = "On";
            else
                AppState = "Off";

            SoundPlayerService.IsMuted = false;

            keyLoggerService.StateChangedEvent += UpdateAppState;
        }

        private void UpdateAppState(object sender, EventArgs e)
        {
            if (sender is KeyLoggerService keyLogger)
            {
                if (keyLogger.State == true)
                    AppState = "On";
                else
                    AppState = "Off";
            }
        }

        private void LoadTranslitTables()
        {
            TranslitTables = transliteratorService.TranslitTables;
            SelectedTranslitTable = settingsService.LastSelectedTranslitTable;
            transliteratorService.TransliterationTableChangedEvent += () => SelectedTranslitTable = transliteratorService.transliterationTableModel.replacementMapFilename;
        }

        [RelayCommand]
        private void ToggleAppState()
        {
            keyLoggerService.State = !keyLoggerService.State;
        }

        [RelayCommand]
        private void ToggleTheme()
        {
            if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                settingsService.ApplicationTheme = ApplicationTheme.Light;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                settingsService.ApplicationTheme = ApplicationTheme.Dark;
            }
        }

        [RelayCommand]
        private static void OpenSettingsWindow()
        {
            // TODO: Rewrite to NavigateToSettingsPage or prevent the creation of multiple windows
            SettingsWindow settings = new();
            settings.Show();
        }

        [RelayCommand]
        private static void OpenSnippetTranslitWindow()
        {
            // TODO: Rewrite to NavigateToSettingsPage or prevent the creation of multiple windows
            SnippetTranslitWindow snippetTranslitWindow = new();
            snippetTranslitWindow.Show();
        }

        [RelayCommand]
        private static void OpenTableViewWindow()
        {
            // TODO: Prevent the creation of multiple windows
            TableViewWindow tableViewWindow = new();
            tableViewWindow.Show();
        }

        [RelayCommand]
        private static void OpenDebugWindow()
        {
            // TODO: Rewrite to NavigateToDebugPage or prevent the creation of multiple windows
            DebugWindow debugWindow = new();
            debugWindow.Show();
        }

        partial void OnWindowStateChanged(WindowState value)
        {
            if (value == WindowState.Minimized)
                IsVisibleInTaskbar = false;
            else
                IsVisibleInTaskbar = true;

            // TODO: Generate notification that the app is in the tray
        }

        partial void OnSelectedTranslitTableChanged(string value)
        {
            transliteratorService.SetTableModel(value);
        }

        partial void OnAppStateChanged(string value)
        {
            if (settingsService.IsToggleSoundOn)
                PlayToggleSound();
        }

        private void PlayToggleSound()
        {
            string pathToSoundToPlay = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Resources/Audio/{(keyLoggerService.State == true ? "cont" : "pause")}.wav");

            if (keyLoggerService.State == true && !string.IsNullOrEmpty(settingsService.PathToCustomToggleOnSound))
                pathToSoundToPlay = settingsService.PathToCustomToggleOnSound;

            if (keyLoggerService.State == false && !string.IsNullOrEmpty(settingsService.PathToCustomToggleOffSound))
                pathToSoundToPlay = settingsService.PathToCustomToggleOffSound;

            SoundPlayerService.Play(pathToSoundToPlay);
        }

        public void SaveSettings()
        {
            settingsService.LastSelectedTranslitTable = SelectedTranslitTable;
            settingsService.ApplicationTheme = (ApplicationTheme)ThemeManager.Current.ApplicationTheme;
            settingsService.Save();
        }

        public void DisposeOfNotifyIcon()
        {
            notifyIcon.Dispose();
        }
    }
}