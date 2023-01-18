using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using TransliteratorWPF_Version.Properties;
using TransliteratorWPF_Version.Views;

namespace TransliteratorWPF_Version
{
    public sealed class LiveTransliterator

    {
        public int state = 1;
        public Transliterator ukrTranslit = new Transliterator();
        public KeyLogger keyLogger;
        public int slowDownKBEInjections = 0;

        public DebugWindow? debugWindow
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

        public bool displayCombos = TransliteratorWPF_Version.Properties.Settings.Default.displayCombos;

        public bool useAlternativeWrite = false;

        private LiveTransliterator()
        {
            keyLogger = KeyLogger.GetInstance();
            keyLogger.LogKeys();

            if (Settings.Default.turnOnTranslitAtStart == false && state == 1)
            {
                ToggleTranslit();
            }
            else if (Settings.Default.turnOnTranslitAtStart == true && state == 0)
            {
                ToggleTranslit();
            }
        }

        private static LiveTransliterator _instance;

        public static LiveTransliterator GetInstance()
        {
            if (_instance == null)
            {
                _instance = new LiveTransliterator();
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
                return repeatCount | (scanCode << 16) | (extendedKey << 24) |
                    (prevKeyState << 30) | (transitionState << 31);
            }
        };

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        public void WriteThroughPostMessage(string text)
        {
            uint vkCode = (byte)(VkKeyScan('q'));

            IntPtr hwnd = GetForegroundWindow();
            debugWindow?.ConsoleLog(hwnd.ToString());

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
            debugWindow?.ConsoleLog("Writing this: " + text);

            foreach (char character in text)
            {
                if (!isTestInput && (debugWindow?.underTestByWinDriverCheckBox.IsChecked == true || keyLogger.gkh.alwaysAllowInjected || !keyLogger.gkh.skipInjected))
                {
                    if (ukrTranslit.alphabet.Contains(character.ToString().ToLower()))
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

            bool MemoryIsNonComboText = (!ukrTranslit.IsStartOfCombination(memory_text)) && !ukrTranslit.EndsWithComboInit(memory_text);

            if (!breakCombo)
            {
                if (MemoryIsNonComboText)
                {
                    TransliterateAndEditIn(lastKey);
                    return true;
                }
                else if (ukrTranslit.LastCharacterIsComboInit(memory_text))
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
            var transliterated_text = ukrTranslit.transliterate(keyLogger.GetMemoryAsString());
            debugWindow?.ConsoleLog($"transliterated version to insert: {transliterated_text}. Original ver.: {keyLogger.GetMemoryAsString()}");
            EditTextbox(last_key, transliterated_text);
        }

        public async void EditTextbox(string lastKey, string transliterated_text)
        {
            if (transliterated_text == "")
            {
                keyLogger.memory.Clear();
                return;
            }

            string keyLoggerMemoryAsString = keyLogger.GetMemoryAsString();

            if (displayCombos && (ukrTranslit.LastCharacterIsComboInit(keyLoggerMemoryAsString) || ukrTranslit.EndsWithBrokenCombo(keyLoggerMemoryAsString) || ukrTranslit.EndsWithComboInit(keyLoggerMemoryAsString)))
            {
                bool shouldNotEraseLastChar = !ukrTranslit.wordenders.Contains(lastKey);
                int nOfEraseSignalsToSend = keyLoggerMemoryAsString.Length - (shouldNotEraseLastChar ? 1 : 0) + keyLogger.nOfKeysOmmittedFromMemory;
                keyLogger.nOfKeysOmmittedFromMemory = 0;

                debugWindow?.ConsoleLog($"Sending {nOfEraseSignalsToSend} erase signals", "Red");

                Write(string.Concat(Enumerable.Repeat("\b", nOfEraseSignalsToSend)) + transliterated_text);
            }
            else
            {
                Write(transliterated_text);
            }

            keyLogger.memory.Clear();
        }

        public void ToggleTranslit()
        {
            keyLogger.Toggle();
        }
    }
}