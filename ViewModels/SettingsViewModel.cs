﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private bool isDisplayOfBufferCharactersEnabled;

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
            IsDisplayOfBufferCharactersEnabled = settingsService.IsDisplayOfBufferCharactersEnabled;
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