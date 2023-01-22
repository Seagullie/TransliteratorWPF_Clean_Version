using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using TransliteratorWPF_Version.Properties;
using TransliteratorWPF_Version.Services;
using TransliteratorWPF_Version.Views;

namespace TransliteratorWPF_Version.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
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
        private HashSet<string> toggleAppStateShortcut;

        private NotifyIcon notifyIcon;
        private readonly LiveTransliterator liveTransliterator;

        public MainViewModel()
        {
            // TODO: Dependency injection 
            liveTransliterator = LiveTransliterator.GetInstance();

            InitializeWindow();
            InitializeNotifyIcon();  
            InitializeStateOverlayWindow();
            InitializeAppState();
            InitializeChangeAppStateShortcut();
            LoadTranslitTables();
        }

        private void InitializeWindow()
        {
            if (Settings.Default.StartMinimized)
                WindowState = WindowState.Minimized;

            if (Settings.Default.SelectedTheme == "Dark")
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            else
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
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

            notifyIcon.Click +=
                delegate (object sender, EventArgs args)
                {
                    WindowState = WindowState.Normal;
                };
        }

        private void InitializeStateOverlayWindow()
        {
            if (Settings.Default.enableStateOverlayWindow)
            {
                StateOverlayWindow stateOveralyWindow = new StateOverlayWindow();
                stateOveralyWindow.Show();
            }
        }

        // TODO: Improve method logic
        private void InitializeAppState()
        {
            if (liveTransliterator.keyLogger.State)
                AppState = "On";
            else
                AppState = "Off";

            liveTransliterator.keyLogger.StateChanged += UpdateAppState;
        }

        private void UpdateAppState(object sender, EventArgs e)
        {
            if (sender is KeyLogger keyLogger)
            {
                if (keyLogger.State == true)
                    AppState = "On";
                else
                    AppState = "Off";
            }
        }
        private void InitializeChangeAppStateShortcut()
        {
            ToggleAppStateShortcut = liveTransliterator.keyLogger.ToggleTranslitShortcut;
            liveTransliterator.keyLogger.ToggleTranslitShortcutChanged += UpdateChangeAppStateShortcut;
        }

        private void LoadTranslitTables()
        {
            TranslitTables = liveTransliterator.ukrTranslit.TranslitTables;
            SelectedTranslitTable = Settings.Default.LastTranslitTable;
        }

        private void UpdateChangeAppStateShortcut(object sender, EventArgs e)
        {
            if (sender is KeyLogger keyLogger)
            {
                ToggleAppStateShortcut = keyLogger.ToggleTranslitShortcut;
            }
        }

        [RelayCommand]
        private void ToggleAppState()
        {
            liveTransliterator.keyLogger.Toggle();
        }

        [RelayCommand]
        private void ToggleTheme()
        {
            if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                Settings.Default.SelectedTheme = "Light";
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                Settings.Default.SelectedTheme = "Dark";
            }
        }

        [RelayCommand]
        private void OpenSettingsWindow()
        {
            // TODO: Rewrite to NavigateToSettingsPage or prevent the creation of multiple windows
            SettingsWindow settings = new SettingsWindow();
            settings.Show();
        }

        [RelayCommand]
        private void OpenTableViewWindow()
        {
            // TODO: Prevent the creation of multiple windows
            TableViewWindow tableViewWindow = new TableViewWindow();
            tableViewWindow.Show();
        }

        [RelayCommand]
        private void OpenDebugWindow()
        {
            // TODO: Rewrite to NavigateToDebugPage or prevent the creation of multiple windows
            DebugWindow debugWindow = new DebugWindow();
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
            liveTransliterator.ukrTranslit.SetReplacementMapFromJson($"{value}");
        }

        partial void OnAppStateChanged(string value)
        {
            // TODO: Dont play sound when initialize?
            if (Settings.Default.PlaySoundOnTranslitToggle)
                PlayToggleSound();

            // TODO: Change state label foreground 
        }

        private void PlayToggleSound()
        {
            Sound sound = new Sound();

            string pathToSoundToPlay = Path.Combine(App.BaseDir, $"Resources/Audio/{(liveTransliterator.keyLogger.State == true ? "cont" : "pause")}.wav");

            if (liveTransliterator.keyLogger.State == true && Settings.Default.pathToCustomToggleOnSound != "")
                pathToSoundToPlay = Settings.Default.pathToCustomToggleOnSound;

            if (liveTransliterator.keyLogger.State == false && Settings.Default.pathToCustomToggleOffSound != "")
                pathToSoundToPlay = Settings.Default.pathToCustomToggleOffSound;

            sound.Play(pathToSoundToPlay);
        }

        // TODO: Find better name for method
        public void SaveSettingsAndDispose()
        {
            Settings.Default.LastTranslitTable = SelectedTranslitTable;
            Settings.Default.Save();

            notifyIcon.Dispose();
        }
    }
}
