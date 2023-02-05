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
        public TransliteratorService ukrTranslit = new();
        public KeyLogger keyLogger;
        public int slowDownKBEInjections = 0;

        private readonly LoggerService loggerService;
        private readonly SettingsService settingsService;

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

        public bool displayCombos;

        public bool useAlternativeWrite = false;

        private Main()
        {
            // TODO: Dependency injection
            keyLogger = KeyLogger.GetInstance();
            // TODO: Refactor
            keyLogger.liveTransliterator = this;
            keyLogger.transliterator = this.ukrTranslit;
            ukrTranslit.TransliterationTableChangedEvent += keyLogger.UpdateWordenders;
            keyLogger.UpdateWordenders();

            keyLogger.HookKeys();

            loggerService = LoggerService.GetInstance();

            settingsService = SettingsService.GetInstance();
            settingsService.Load();
            displayCombos = settingsService.IsBufferInputEnabled;

            keyLogger.State = settingsService.IsTranslitEnabledAtStartup;
        }

        private static Main _instance;

        public static Main GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Main();
            }
            return _instance;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool PostMessageW(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, uint wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern short VkKeyScan(char ch);

        private const uint MAPVK_VK_TO_VSC = 0x00;

        private class extraKeyInfo
        {
            public ushort repeatCount;
            public char scanCode;
            public ushort extendedKey, prevKeyState, transitionState;

            public int getint()
            {
                return repeatCount | scanCode << 16 | extendedKey << 24 |
                    prevKeyState << 30 | transitionState << 31;
            }
        };

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        public void WriteThroughPostMessage(string text)
        {
            uint vkCode = (byte)VkKeyScan('q');

            IntPtr hwnd = GetForegroundWindow();
            loggerService.LogMessage(this, hwnd.ToString());

            extraKeyInfo lParam = new extraKeyInfo();

            lParam.scanCode = (char)MapVirtualKey(vkCode, MAPVK_VK_TO_VSC);
            lParam.repeatCount = 1;
            lParam.prevKeyState = 1;
            lParam.transitionState = 1;

            PostMessage(hwnd, WM_KEYDOWN, vkCode, lParam.getint());

            for (int i = 0; i < text.Length; i++)
            {
            }
        }

        public async void Write(string text, bool isTestInput = false)
        {
            loggerService.LogMessage(this, "Writing this: " + text);

            foreach (char character in text)
            {
                if (!isTestInput && (debugWindow?.underTestByWinDriverCheckBox.IsChecked == true || keyLogger.gkh.alwaysAllowInjected || !keyLogger.gkh.skipInjected))
                {
                    if (ukrTranslit.transliterationTableModel.alphabet.Contains(character.ToString().ToLower()))
                    {
                        keyLogger.keysToIgnore.Add(character.ToString());
                    }
                }

                if (slowDownKBEInjections != 0)
                {
                    await Task.Delay(slowDownKBEInjections);
                }

                WinAPI.WriteStringThroughSendInput(character.ToString());
            }
        }

        public async void WriteThroughAnotherProcess(string text)
        {
            keyLogger.gkh.skipInjected = false;

            // warning: hardcoded
            string pathToKeyboardInputSimulator = @"KeyboardInputSimulator\KeyboardInputSimulator.exe";
            Process simulatorProcess = Process.Start($@"{pathToKeyboardInputSimulator}", $"{text} 200");
            simulatorProcess.EnableRaisingEvents = true;
            simulatorProcess.Exited += (o, i) =>
            {
                keyLogger.gkh.skipInjected = true;
            };
        }

        public async Task WriteInjected(string text, int delayBetweenEachChar = 300)
        {
            keyLogger.gkh.skipInjected = false;

            foreach (char c in text)
            {
                Write(c.ToString(), true);
                await Task.Delay(delayBetweenEachChar);
            }

            keyLogger.gkh.skipInjected = true;
        }

        public async void WriteCharThroughSendKey(string chr)
        {
            if (debugWindow?.underTestByWinDriverCheckBox.IsChecked == true || keyLogger.gkh.alwaysAllowInjected)
            {
                if (chr == "'")
                {
                    keyLogger.keysToIgnore.Add(chr);
                }
            }

            SendKeys.Send(chr);
        }

        public async Task WriteThroughSendKeys(string text)
        {
            if (text.StartsWith("{"))
            {
                SendKeys.Send(text);
            }
            else
            {
                foreach (char chr in text)
                {
                    if (slowDownKBEInjections != 0)
                    {
                        await Task.Delay(slowDownKBEInjections);
                    }

                    WriteCharThroughSendKey(chr.ToString());
                }
            }
        }

        public bool Transliterate(string lastKey, bool breakCombo = false)
        {
            if (state == 0)
            {
                return false;
            }

            var memory_text = keyLogger.GetMemoryAsString().ToLower();

            TableKeyAnalyzerService keyAnalyzer = ukrTranslit.tableKeyAnalayzerService;
            bool MemoryIsNonComboText = !keyAnalyzer.IsStartOfCombination(memory_text) && !keyAnalyzer.EndsWithComboInit(memory_text);

            if (!breakCombo)
            {
                if (MemoryIsNonComboText)
                {
                    TransliterateAndEditIn(lastKey);
                    return true;
                }
                else if (ukrTranslit.tableKeyAnalayzerService.LastCharacterIsComboInit(memory_text))
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
            var transliterated_text = ukrTranslit.Transliterate(keyLogger.GetMemoryAsString());
            loggerService.LogMessage(this, $"transliterated version to insert: {transliterated_text}. Original ver.: {keyLogger.GetMemoryAsString()}");
            EditTextbox(last_key, transliterated_text);
        }

        public void EditTextbox(string lastKey, string transliterated_text)
        {
            if (transliterated_text == "")
            {
                keyLogger.memory.Clear();
                return;
            }

            string keyLoggerMemoryAsString = keyLogger.GetMemoryAsString();

            TableKeyAnalyzerService keyAnalyzer = ukrTranslit.tableKeyAnalayzerService;

            if (displayCombos && (keyAnalyzer.LastCharacterIsComboInit(keyLoggerMemoryAsString) || keyAnalyzer.EndsWithBrokenCombo(keyLoggerMemoryAsString) || keyAnalyzer.EndsWithComboInit(keyLoggerMemoryAsString)))
            {
                bool shouldNotEraseLastChar = !keyLogger.wordenders.Contains(lastKey);
                int nOfEraseSignalsToSend = keyLoggerMemoryAsString.Length - (shouldNotEraseLastChar ? 1 : 0) + keyLogger.nOfKeysOmmittedFromMemory;
                keyLogger.nOfKeysOmmittedFromMemory = 0;

                loggerService.LogMessage(this, $"Sending {nOfEraseSignalsToSend} erase signals", "Red");

                Write(string.Concat(Enumerable.Repeat("\b", nOfEraseSignalsToSend)) + transliterated_text);
            }
            else
            {
                Write(transliterated_text);
            }

            keyLogger.memory.Clear();
        }
    }
}