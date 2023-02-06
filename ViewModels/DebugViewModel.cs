using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TransliteratorWPF_Version.Services;
using TransliteratorWPF_Version.Views;

namespace TransliteratorWPF_Version.ViewModels
{
    public partial class DebugViewModel : ObservableObject
    {
        private SoundPlayer soundCont;

        private SoundPlayer soundPause;

        public bool logsEnabled = true;

        private readonly Main liveTransliterator;
        private readonly SettingsService settingsService;
        private LoggerService loggerService;

        // TODO: Rewrite to bool
        [ObservableProperty]
        private string appState;

        public DebugViewModel()
        {
            // TODO: Dependency injection
            settingsService = SettingsService.GetInstance();
            loggerService = LoggerService.GetInstance();

            string str = "TransliteratorWPF_Version.Resources.Audio.cont.wav";
            Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(str);
            soundCont = new(s);

            str = "TransliteratorWPF_Version.Resources.Audio.pause.wav";
            s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(str);
            soundPause = new(s);
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
            string stateDesc = (liveTransliterator.keyLogger.State == true ? "On" : "Off");

            if ((settingsService.IsToggleSoundOn))
            {
                SoundPlayer soundToPlay = liveTransliterator.keyLogger.State == true ? soundCont : soundPause;
                soundToPlay.Play();
            }

            loggerService.LogMessage(this, $"Translit {stateDesc}");
            AppState = stateDesc;
            if (liveTransliterator.keyLogger.State == true)
            {
                // TODO: Update State Label Foreground
                //StateLabel.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                // TODO: Update State Label Foreground
                //StateLabel.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
    }
}