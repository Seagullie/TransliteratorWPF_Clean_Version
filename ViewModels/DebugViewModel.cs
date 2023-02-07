using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModernWpf;
using System.Collections.Generic;
using TransliteratorWPF_Version.Helpers;
using TransliteratorWPF_Version.Services;
using TransliteratorWPF_Version.Views;

namespace TransliteratorWPF_Version.ViewModels
{
    public partial class DebugViewModel : ObservableObject
    {
        public bool logsEnabled = true;

        private Main liveTransliterator;
        private TransliteratorService transliteratorService;
        private LoggerService loggerService;
        private KeyLoggerService keyLoggerService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AppStateDesc))]
        private bool appState;

        public string AppStateDesc { get => AppState ? "On" : "Off"; }

        [ObservableProperty]
        private List<string> translitTables;

        [ObservableProperty]
        private string selectedTranslitTable;

        // TODO: Refactor
        partial void OnSelectedTranslitTableChanged(string value)
        {
            transliteratorService.SetTableModel(value);
        }

        public DebugViewModel()
        {
            // TODO: Dependency injection
            loggerService = LoggerService.GetInstance();
            liveTransliterator = Main.GetInstance();
            keyLoggerService = KeyLoggerService.GetInstance();
            transliteratorService = TransliteratorService.GetInstance();

            // ---

            TranslitTables = transliteratorService.TranslitTables;

            keyLoggerService.StateChangedEvent += (s, e) => AppState = keyLoggerService.State;

            // TODO: Convert it into two-way binding by configuring "Path" property of XAML binding and targeting property
            // of TransliteratorService.
            // Is it ok to make service property observable?
            // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/binding-declarations-overview?view=netdesktop-6.0
            transliteratorService.TransliterationTableChangedEvent += () => SelectedTranslitTable = transliteratorService.transliterationTableModel.replacementMapFilename;

            SelectedTranslitTable = transliteratorService.transliterationTableModel.replacementMapFilename;

            // TODO: Subscribe to TranslitTablesChanged event
        }

        [RelayCommand]
        private void ToggleLogs()
        {
            logsEnabled = !logsEnabled;
        }

        [RelayCommand]
        private void ToggleTranslit()
        {
            liveTransliterator.keyLogger.State = !liveTransliterator.keyLogger.State;
            string stateDesc = liveTransliterator.keyLogger.State ? "enabled" : "disabled";

            loggerService.LogMessage(this, $"Translit {stateDesc}");
        }

        [RelayCommand]
        private void OpenSettingsWindow()
        {
            SettingsWindow settingsWindow = new();
            settingsWindow.Show();
        }

        [RelayCommand]
        private void ShowTranslitTable()
        {
            string serializedTable = Newtonsoft.Json.JsonConvert.SerializeObject(liveTransliterator.transliteratorService.transliterationTableModel.replacementTable, Newtonsoft.Json.Formatting.Indented);
            loggerService.LogMessage(this, "\n" + serializedTable);
        }

        [RelayCommand]
        private void GetKeyLoggerMemory()
        {
            string memory = liveTransliterator.keyLogger.GetMemoryAsString();
            loggerService.LogMessage(this, $"Key Logger memory: [{memory}]");
        }

        [RelayCommand]
        private void AllowInjectedKeys()
        {
            liveTransliterator.keyLogger.gkh.alwaysAllowInjected = true;
        }

        [RelayCommand]
        private void SlowDownKBEInjections()
        {
            liveTransliterator.slowDownKBEInjections = 2000;
        }

        [RelayCommand]
        private void GetLayout()
        {
            string layout = Utilities.GetCurrentKbLayout();
            loggerService.LogMessage(this, layout);
        }

        [RelayCommand]
        private void CheckCaseButtons()
        {
            bool isCAPSLOCKon = liveTransliterator.keyLogger.keyStateChecker.IsCAPSLOCKon();
            bool isShiftPressedDown = liveTransliterator.keyLogger.keyStateChecker.IsShiftPressedDown();

            loggerService.LogMessage(this, $"CAPS LOCK on: {isCAPSLOCKon}. SHIFT pressed down: {isShiftPressedDown}");
        }

        // TODO: Move to own service
        [RelayCommand]
        private void ChangeTheme()
        {
            if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }
        }
    }
}