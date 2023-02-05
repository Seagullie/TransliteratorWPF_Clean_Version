using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using TransliteratorWPF_Version.Services;

namespace TransliteratorWPF_Version.ViewModels
{
    public partial class StateOverlayViewModel : ObservableObject
    {
        private readonly SettingsService settingsService;
        private readonly KeyLoggerService keyLogger;

        [ObservableProperty]
        private bool isStateOverlayEnabled;

        [ObservableProperty]
        private bool appState;

        public StateOverlayViewModel()
        {
            settingsService = SettingsService.GetInstance();
            keyLogger = KeyLoggerService.GetInstance();

            isStateOverlayEnabled = settingsService.IsStateOverlayEnabled;

            settingsService.SettingsSavedEvent += (sender, eventObj) => IsStateOverlayEnabled = settingsService.IsStateOverlayEnabled;
            keyLogger.StateChangedEvent += (sender, eventObj) => AppState = keyLogger.State;

            AppState = keyLogger.State;
        }
    }
}