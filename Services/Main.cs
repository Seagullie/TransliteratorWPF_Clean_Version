using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using TransliteratorWPF_Version.Helpers;
using TransliteratorWPF_Version.Views;

namespace TransliteratorWPF_Version.Services
{
    public sealed class Main
    {
        public int state = 1;
        public int slowDownKBEInjections = 0;
        public bool useAlternativeWrite = false;
        public bool displayCombos;

        public readonly TransliteratorService transliteratorService;
        public readonly KeyLoggerService keyLogger;
        private readonly LoggerService loggerService;
        private readonly SettingsService settingsService;
        public readonly KeyInjectorService keyInjectorService;

        private Main()
        {
            // TODO: Dependency injection
            transliteratorService = TransliteratorService.GetInstance();
            keyLogger = KeyLoggerService.GetInstance();
            // TODO: Refactor
            keyLogger.liveTransliterator = this;
            keyLogger.transliterator = this.transliteratorService;
            transliteratorService.TransliterationTableChangedEvent += keyLogger.UpdateWordenders;
            keyLogger.UpdateWordenders();

            keyLogger.HookKeys();

            loggerService = LoggerService.GetInstance();
            keyInjectorService = KeyInjectorService.GetInstance();

            settingsService = SettingsService.GetInstance();
            settingsService.Load();
            displayCombos = settingsService.IsBufferInputEnabled;

            keyLogger.State = settingsService.IsTranslitEnabledAtStartup;
        }

        private static Main _instance;

        public static Main GetInstance()
        {
            _instance ??= new Main();
            return _instance;
        }

        public bool Transliterate(string lastKey, bool breakCombo = false)
        {
            if (state == 0)
            {
                return false;
            }

            var memory_text = keyLogger.GetMemoryAsString().ToLower();

            TableKeyAnalyzerService keyAnalyzer = transliteratorService.tableKeyAnalayzerService;
            bool MemoryIsNonComboText = !keyAnalyzer.IsStartOfCombination(memory_text) && !keyAnalyzer.EndsWithComboInit(memory_text);

            if (!breakCombo)
            {
                if (MemoryIsNonComboText)
                {
                    TransliterateAndEditIn(lastKey);
                    return true;
                }
                else if (transliteratorService.tableKeyAnalayzerService.LastCharacterIsComboInit(memory_text))
                {
                    TransliterateAndEditIn(lastKey);
                    return true;
                }
            }
            else if (breakCombo)
            {
                TransliterateAndEditIn(lastKey);
                return true;
            }

            return false;
        }

        public void TransliterateAndEditIn(string last_key)
        {
            var transliterated_text = transliteratorService.Transliterate(keyLogger.GetMemoryAsString());
            loggerService.LogMessage(this, $"transliterated version to insert: {transliterated_text}. Original ver.: {keyLogger.GetMemoryAsString()}");
            EditTextbox(last_key, transliterated_text);
        }

        // the textbox could be both within and outside the app
        public void EditTextbox(string lastKey, string transliteratedText)
        {
            if (transliteratedText == "")
            {
                keyLogger.memory.Clear();
                return;
            }

            string keyLoggerMemoryAsString = keyLogger.GetMemoryAsString();

            TableKeyAnalyzerService keyAnalyzer = transliteratorService.tableKeyAnalayzerService;

            if (displayCombos && (keyAnalyzer.LastCharacterIsComboInit(keyLoggerMemoryAsString) || keyAnalyzer.EndsWithBrokenCombo(keyLoggerMemoryAsString) || keyAnalyzer.EndsWithComboInit(keyLoggerMemoryAsString)))
            {
                bool shouldNotEraseLastChar = !keyLogger.wordenders.Contains(lastKey);
                int nOfEraseSignalsToSend = keyLoggerMemoryAsString.Length - (shouldNotEraseLastChar ? 1 : 0) + keyLogger.nOfKeysOmmittedFromMemory;
                keyLogger.nOfKeysOmmittedFromMemory = 0;

                loggerService.LogMessage(this, $"Sending {nOfEraseSignalsToSend} erase signals", "Red");

                keyInjectorService.Write(string.Concat(Enumerable.Repeat("\b", nOfEraseSignalsToSend)) + transliteratedText);
            }
            else
            {
                keyInjectorService.Write(transliteratedText);
            }

            keyLogger.memory.Clear();
        }
    }
}