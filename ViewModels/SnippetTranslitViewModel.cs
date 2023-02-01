using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Windows;
using TransliteratorWPF_Version.Services;
using TransliteratorWPF_Version.Views;

namespace TransliteratorWPF_Version.ViewModels
{
    public partial class SnippetTranslitViewModel : ObservableObject
    {
        private readonly SettingsService settingsService;
        private readonly KeyLogger keyLogger;
        private Transliterator transliterator;

        [ObservableProperty]
        private string userInput;

        [ObservableProperty]
        private string transliterationResults;

        public SnippetTranslitViewModel()
        {
            settingsService = SettingsService.GetInstance();
            keyLogger = KeyLogger.GetInstance();
            transliterator = keyLogger.transliterator;
        }

        [RelayCommand]
        private void TransliterateSnippet()
        {
            string textToTransliterate = UserInput;
            string transliterationResults = transliterator.Transliterate(textToTransliterate);
            TransliterationResults = transliterationResults;
        }
    }
}