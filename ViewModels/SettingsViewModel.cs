using CommunityToolkit.Mvvm.ComponentModel;
using System;
using TransliteratorWPF_Version.Properties;

namespace TransliteratorWPF_Version.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        // TODO: Find better name for this property
        [ObservableProperty]
        private bool toggleSoundIsOn;

        [ObservableProperty]
        private string toggleTranslitShortcut;

        public SettingsViewModel()
        {
            InitializePropertiesFromSettings();
        }

        private void InitializePropertiesFromSettings()
        {
            ToggleSoundIsOn = Settings.Default.PlaySoundOnTranslitToggle;
            ToggleTranslitShortcut = Settings.Default.ToggleTranslitShortcut;
        }

        partial void OnToggleSoundIsOnChanged(bool value)
        {
            Settings.Default.PlaySoundOnTranslitToggle = value;
        }


    }
}
