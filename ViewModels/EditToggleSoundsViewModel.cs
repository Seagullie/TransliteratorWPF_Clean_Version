using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using TransliteratorWPF_Version.Services;

namespace TransliteratorWPF_Version.ViewModels
{
    public partial class EditToggleSoundsViewModel : ObservableObject
    {
        private readonly SettingsService settingsService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ToggleOnSoundFileName))]
        private string toggleOnSoundFilePath;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ToggleOffSoundFileName))]
        private string toggleOffSoundFilePath;

        public string ToggleOnSoundFileName { get => Path.GetFileName(ToggleOnSoundFilePath) ?? "<None>"; }

        public string ToggleOffSoundFileName { get => Path.GetFileName(ToggleOffSoundFilePath) ?? "<None>"; }


        public EditToggleSoundsViewModel()
        {
            settingsService = SettingsService.GetInstance();
            ToggleOnSoundFilePath = settingsService.PathToCustomToggleOnSound;
            ToggleOffSoundFilePath = settingsService.PathToCustomToggleOffSound;
        }

        [RelayCommand]
        private void ChangeToggleOnSound()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Audio documents (.wav)|*.wav" // Filter files by extension
            };

            bool? result = dialog.ShowDialog();

            if (result != null && result == true)
            {
                // Open document
                string pathToFile = dialog.FileName;
                ToggleOnSoundFilePath = pathToFile;
                settingsService.PathToCustomToggleOnSound = pathToFile;
            }
        }

        [RelayCommand]
        private void ChangeToggleOffSound()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Audio documents (.wav)|*.wav" // Filter files by extension
            };

            bool? result = dialog.ShowDialog();

            if (result != null && result == true)
            {
                // Open document
                string pathToFile = dialog.FileName;
                ToggleOffSoundFilePath = pathToFile;
                settingsService.PathToCustomToggleOffSound = pathToFile;
            }
        }

        [RelayCommand]
        private void DeleteToggleOnSound()
        {
            ToggleOnSoundFilePath = null;
            settingsService.PathToCustomToggleOnSound = null;
        }

        [RelayCommand]
        private void DeleteToggleOffSound()
        {
            ToggleOffSoundFilePath = null;
            settingsService.PathToCustomToggleOffSound = null;
        }
    }
}
