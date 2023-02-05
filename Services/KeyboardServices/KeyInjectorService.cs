using System.Threading.Tasks;
using System.Windows;
using TransliteratorWPF_Version.Helpers;
using TransliteratorWPF_Version.Services.KeyboardServices;
using TransliteratorWPF_Version.Views;

namespace TransliteratorWPF_Version.Services
{
    public class KeyInjectorService : KeyInjectorBase
    {
        private readonly KeyLoggerService keyLoggerService;
        private readonly TransliteratorService transliteratorService;

        public static KeyInjectorService GetInstance()
        {
            _instance ??= new KeyInjectorService();
            return _instance;
        }

        public DebugWindow debugWindow
        {
            get
            {
                WindowCollection windows = System.Windows.Application.Current.Windows;
                foreach (Window window in windows)
                {
                    // warning: hardcoded
                    if (window.Name == "DebugWindow1")
                    {
                        return (DebugWindow)window;
                    }
                }
                return null;
            }
        }

        private static KeyInjectorService _instance;

        private KeyInjectorService()
        {
            keyLoggerService = KeyLoggerService.GetInstance();
            transliteratorService = TransliteratorService.GetInstance();
        }

        public async void Write(string text, bool isTestInput = false)
        {
            loggerService.LogMessage(this, "Writing this: " + text);

            foreach (char character in text)
            {
                // TODO: Refactor
                if (!isTestInput && (debugWindow?.underTestByWinDriverCheckBox.IsChecked == true || keyLoggerService.gkh.alwaysAllowInjected || !keyLoggerService.gkh.skipInjected))
                {
                    if (transliteratorService.transliterationTableModel.alphabet.Contains(character.ToString().ToLower()))
                    {
                        keyLoggerService.keysToIgnore.Add(character.ToString());
                    }
                }

                if (KBEInjectionsIntervalMilliseconds != 0)
                {
                    await Task.Delay(KBEInjectionsIntervalMilliseconds);
                }

                WinAPI.WriteStringThroughSendInput(character.ToString());
            }
        }

        // by default injected keys are skipped so that injected transliteration doesn't come back to keylogger
        // but, in some testing scenarios we need to allow them
        public async Task WriteInjected(string text, int delayBetweenEachChar = 300)
        {
            keyLoggerService.gkh.skipInjected = false;

            foreach (char c in text)
            {
                Write(c.ToString(), true);
                await Task.Delay(delayBetweenEachChar);
            }

            keyLoggerService.gkh.skipInjected = true;
        }
    }
}