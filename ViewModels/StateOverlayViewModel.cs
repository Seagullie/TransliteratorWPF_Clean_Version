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
        private readonly KeyLogger keyLogger;

        [ObservableProperty]
        private bool isStateOverlayEnabled;

        // TODO: Fix "BindingExpression path error: 'АppStateFuck' property not found on 'object' ''StateOverlayViewModel'" error
        [ObservableProperty]
        private bool appStateFuck;

        public StateOverlayViewModel()
        {
            settingsService = SettingsService.GetInstance();
            keyLogger = KeyLogger.GetInstance();

            isStateOverlayEnabled = settingsService.IsStateOverlayEnabled;
            //keyLogger.StateChangedEvent += (sender, eventObj) => appState = keyLogger.State;
            keyLogger.StateChangedEvent += UpdateAppState;

            AppStateFuck = keyLogger.State;
        }

        public void UpdateAppState(object sender, EventArgs eventArgs)
        {
            AppStateFuck = keyLogger.State;
        }
    }
}