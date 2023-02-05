using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using TransliteratorWPF_Version.Helpers;
using TransliteratorWPF_Version.Models;
using TransliteratorWPF_Version.Views;
using Application = System.Windows.Application;
using Key = System.Windows.Input.Key;

namespace TransliteratorWPF_Version.Services
{
    public sealed partial class KeyLogger
    {
        private bool state;
        public bool State { get => state; set => SetState(value); }

        public Main liveTransliterator;
        public TransliteratorService transliterator;

        private readonly LoggerService loggerService;
        private readonly SettingsService settingsService;

        public event EventHandler StateChangedEvent;

        public event EventHandler ToggleTranslitShortcutChanged;

        public DebugWindow debugWindow
        {
            get
            {
                WindowCollection windows = Application.Current.Windows;
                foreach (Window window in windows)
                {
                    if (window.Name == "DebugWindow1")
                    {
                        return (DebugWindow)window;
                    }
                }
                return null;
            }
        }

        private KeyEventHandler gkh_KeyDownHandler;

        public List<string> wordendersVanilla { get; private set; }

        public List<string> memory = new List<string>();

        public KeyStateChecker keyStateChecker = new KeyStateChecker();

        public List<string> keysToIgnore = new List<string> { };

        public GlobalKeyboardHook gkh;

        public HashSet<string> ToggleTranslitShortcut = new HashSet<string>();

        public HashSet<string> ChangeLanguageShortcut = new HashSet<string> { "Alt", "LShiftKey" };

        public HashSet<string> ChangeLanguageShortcutAlt = new HashSet<string> { "LMenu", "Shift" };

        public string ToggleTranslitShortcutMainKey;
        public int nOfKeysOmmittedFromMemory = 0;

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        private KeyLogger()
        {
            // TODO: Dependency injection
            loggerService = LoggerService.GetInstance();

            settingsService = SettingsService.GetInstance();
            settingsService.Load();
            settingsService.SettingsSavedEvent += UpdateSettings;

            gkh = new GlobalKeyboardHook();

            FetchToggleTranslitShortcutFromSettings();
            gkh_KeyDownHandler = new KeyEventHandler(Gkh_KeyDown);

            // TODO: explain wordendersVanilla or rename the variable to something more descriptive
            wordendersVanilla = wordenders.ToList();
        }

        // TODO: rename
        public void UpdateWordenders()
        {
            // excluding alphabet keys from wordenders

            wordenders = wordendersVanilla;
            HashSet<string> alphabet = liveTransliterator.ukrTranslit.transliterationTableModel.alphabet;

            foreach (string alphabetLetter in alphabet)
            {
                if (wordenders.Contains(alphabetLetter))
                {
                    wordenders.Remove(alphabetLetter);
                }
            }
        }

        private static KeyLogger _instance;

        public static KeyLogger GetInstance()
        {
            _instance ??= new KeyLogger();
            return _instance;
        }

        // those are the symbols that never occur somewhere within regular words, but rather at the end
        // majority of these are punctuation marks
        public List<string> wordenders = new List<string> {
                "0",
                ";",
                ":",
                "!",
                ")",
                "]",
                ",",
                ".",
                "\"",
                @"\",
                "/",
                "}",
                ">",
                "?",
                "-",
                "_",
                "*",
                "space",
                " ",
                "enter",
                // one wild enter incoming:
                @"
",
                "EOF",
            };

        private void SetState(bool value)
        {
            if (value == state)
                return;

            state = value;

            if (!state)
                memory.Clear();

            StateChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateSettings(object sender, EventArgs eventArgs)
        {
            FetchToggleTranslitShortcutFromSettings();
        }

        public void FetchToggleTranslitShortcutFromSettings()
        {
            string SettingsString = settingsService.ToggleHotKey.ToString();
            string[] SettingStringAsArray;
            if (SettingsString.Contains('+'))
            {
                SettingStringAsArray = SettingsString.Split(" + ".ToCharArray()).Where(k => k != "").ToArray();
            }
            else
            {
                SettingStringAsArray = SettingsString.Split(", ".ToCharArray()).Where(k => k != "").ToArray();
            }

            ToggleTranslitShortcutMainKey = SettingStringAsArray[SettingStringAsArray.Length - 1];
            ToggleTranslitShortcut = SettingStringAsArray.ToHashSet();
            ToggleTranslitShortcutChanged?.Invoke(this, EventArgs.Empty);
        }

        public string GetMemoryAsString()
        {
            return string.Join("", memory);
        }

        public void HookKeys()
        {
            // add latin alphabet
            foreach (int value in Enumerable.Range(65, 25 + 1))
            {
                gkh.HookedKeys.Add((Keys)value);
            }

            // add numbers
            foreach (int value in Enumerable.Range(48, 10))
            {
                gkh.HookedKeys.Add((Keys)value);
            }

            gkh.HookedKeys.Add(Keys.Back);

            // add wordenders
            gkh.HookedKeys.Add(Keys.OemSemicolon);
            gkh.HookedKeys.Add(Keys.Space);
            gkh.HookedKeys.Add(Keys.Enter);
            gkh.HookedKeys.Add(Keys.Alt);
            gkh.HookedKeys.Add(Keys.Shift);

            gkh.HookedKeys.Add(Keys.OemQuestion);
            gkh.HookedKeys.Add(Keys.Oemcomma);
            gkh.HookedKeys.Add(Keys.OemPeriod);
            gkh.HookedKeys.Add(Keys.OemCloseBrackets);
            gkh.HookedKeys.Add(Keys.OemOpenBrackets);
            gkh.HookedKeys.Add(Keys.OemQuotes);
            gkh.HookedKeys.Add(Keys.OemMinus);
            gkh.KeyDown += gkh_KeyDownHandler;
        }

        public string KeyCodeToUnicode(Keys key)
        {
            byte[] keyboardState = new byte[255];
            bool keyboardStateStatus = GetKeyboardState(keyboardState);

            if (!keyboardStateStatus)
            {
                return "";
            }

            uint virtualKeyCode = (uint)key;
            uint scanCode = MapVirtualKey(virtualKeyCode, 0);
            IntPtr inputLocaleIdentifier = GetKeyboardLayout(0);

            StringBuilder result = new StringBuilder();
            _ = ToUnicodeEx(virtualKeyCode, scanCode, keyboardState, result, 5, 0, inputLocaleIdentifier);

            return result.ToString();
        }

        private bool HandleShortcuts(KeyEventArgs e)
        {
            string key = e.KeyCode.ToString();

            bool altDown = keyStateChecker.IsKeyDown(Key.LeftAlt);
            bool leftShiftDown = keyStateChecker.IsKeyDown(Key.LeftShift);

            if (key != ToggleTranslitShortcutMainKey && !altDown && !leftShiftDown)
            {
                return false;
            }

            string keyString = e.KeyData.ToString().ToLower();

            bool ctrlDown = keyStateChecker.IsKeyDown(Key.LeftCtrl);

            loggerService.LogMessage(this, $"ALT down: {altDown}. KeyString: {keyString}");

            HashSet<string> registeredKeyStrokesAsHash = new HashSet<string>();
            if (altDown)
            {
                registeredKeyStrokesAsHash.Add("Alt");
            }
            if (ctrlDown)
            {
                registeredKeyStrokesAsHash.Add("Control");
            }
            if (leftShiftDown)
            {
                registeredKeyStrokesAsHash.Add("Shift");
            }

            registeredKeyStrokesAsHash.Add(key);

            if (registeredKeyStrokesAsHash.SetEquals(ToggleTranslitShortcut))
            {
                // TODO: Try udnerstand this logic and do code refactoring
                State = !State;
                e.Handled = true;
                return true;
            }

            // I don't udnerstand why it even works if shift isn't pressed simultaneously with alt, but rather a moment later, which makes catching shift as main key more favourable
            else if (!settingsService.IsAltShiftGlobalShortcutEnabled && (registeredKeyStrokesAsHash.SetEquals(ChangeLanguageShortcut) || registeredKeyStrokesAsHash.SetEquals(ChangeLanguageShortcutAlt)))
            {
                e.Handled = true;
                return true;
            }

            return false;
        }

        private void Gkh_KeyDown(object sender, KeyEventArgs e)
        {
            loggerService.LogMessage(this, "key pressed");

            if (HandleShortcuts(e)) return;

            if (State == false)
            {
                loggerService.LogMessage(this, "Translit off. Key skipped");
                return;
            }

            var UnicodeChar = KeyCodeToUnicode(e.KeyCode);

            if (UnicodeChar.Length == 0) return;

            // TODO: explain difference between shouldIgnoreKey & ShouldSkipKey
            bool shouldIgnoreKey = (debugWindow?.underTestByWinDriverCheckBox.IsChecked == true || gkh.alwaysAllowInjected || gkh.skipInjected != true) && keysToIgnore.Count > 0 && keysToIgnore[0] == UnicodeChar;

            if (shouldIgnoreKey)
            {
                keysToIgnore.RemoveAt(0);
                loggerService.LogMessage(this, $"ignoring this key: {UnicodeChar}");
                return;
            }

            loggerService.LogMessage(this, $"{e.KeyCode} (Unicode: {UnicodeChar}) DOWN");

            if (ShouldClearMemory())
            {
                memory.Clear();
                return;
            }

            if (e.KeyCode.ToString() == "Back")
            {
                HandleBackspace();
                loggerService.LogMessage(this, $"Handling {e.KeyCode}");
                return;
            }

            string lowerCaseKeyCode = e.KeyCode.ToString().ToLower();

            if (ShouldSkipKey(UnicodeChar) && !IsWordender(lowerCaseKeyCode, UnicodeChar))
            {
                loggerService.LogMessage(this, $"Skipping {e.KeyCode}");
                return;
            }

            if (memory.Count > 20) memory.Clear();

            bool isLowerCase = keyStateChecker.IsLowerCase();
            string caseSensitiveCharacter = isLowerCase ? UnicodeChar.ToLower() : UnicodeChar.ToUpper();
            loggerService.LogMessage(this, $"Keycode: {caseSensitiveCharacter}. KeyData: {e.KeyData} DOWN.");

            if (IsWordender(lowerCaseKeyCode, UnicodeChar))
            {
                HandleWordenderKey(UnicodeChar, ref e);
                return;
            }

            if (liveTransliterator.displayCombos)
            {
                // transliterate without buffering input. Hard and sometimes buggy, but more fluid and snappy
                TransliterateWithoutBuffering(caseSensitiveCharacter, ref e);
            }
            else
            {
                // buffer input untill transliteration can be made. Simple and reliable

                e.Handled = true;

                memory.Add(caseSensitiveCharacter);
                liveTransliterator.Transliterate(caseSensitiveCharacter);
            }
        }

        // TODO: Come up with better name for the method
        public void TransliterateWithoutBuffering(string caseSensitiveCharacter, ref KeyEventArgs e)
        {
            DecideOnKeySuppression(caseSensitiveCharacter.ToLower(), ref e);

            string upcomingText = (GetMemoryAsString() + caseSensitiveCharacter).ToLower();

            // TODO: Figure out how to shorten those long property chains
            if (transliterator.tableKeyAnalayzerService.EndsWithBrokenCombo(upcomingText) && transliterator.tableKeyAnalayzerService.EndsWithComboInit(upcomingText) && !transliterator.tableKeyAnalayzerService.IsPartOfCombination(upcomingText))
            {
                nOfKeysOmmittedFromMemory = 1;
                liveTransliterator.Transliterate(caseSensitiveCharacter, true);
                memory.Add(caseSensitiveCharacter);
                liveTransliterator.Write(caseSensitiveCharacter);
            }
            else
            {
                memory.Add(caseSensitiveCharacter);
                liveTransliterator.Transliterate(caseSensitiveCharacter);
            }
        }

        public bool IsWordender(string lowerCaseKeyCode, string UnicodeChar)
        {
            return wordenders.Contains(lowerCaseKeyCode) || wordenders.Contains(UnicodeChar.ToLower());
        }

        // not sure whether this method is necessary when buffer mode is on. Perhaps should separate it from the class
        public void HandleWordenderKey(string UnicodeChar, ref KeyEventArgs e)
        {
            if (memory.Count == 0)
            {
                return;
            }

            e.Handled = true;

            liveTransliterator.Transliterate(UnicodeChar, true);
            liveTransliterator.Write(UnicodeChar);
        }

        // TODO: Move to separate class
        public bool DecideOnKeySuppression(string caseSensitiveCharacter, ref KeyEventArgs e)
        {
            ref TransliteratorService translit = ref liveTransliterator.ukrTranslit;
            TableKeyAnalyzerService keyAnalyzer = translit.tableKeyAnalayzerService;

            if (keyAnalyzer.IsPartOfCombination(caseSensitiveCharacter))
            {
                string upcomingText = (GetMemoryAsString() + caseSensitiveCharacter).ToLower();

                if (keyAnalyzer.IsSubComboFinisher(caseSensitiveCharacter))
                {
                    e.Handled = false;
                }
                else if (keyAnalyzer.IsCombo(upcomingText))
                {
                    e.Handled = true;
                    loggerService.LogMessage(this, $"{caseSensitiveCharacter} – is full combo finisher");
                }
                else if (keyAnalyzer.EndsWithBrokenCombo(upcomingText) && !keyAnalyzer.IsPartOfCombination(upcomingText))
                {
                    e.Handled = true;
                }
                else if (!keyAnalyzer.IsStartOfCombination(caseSensitiveCharacter))
                {
                    if (upcomingText.Length > 1 && keyAnalyzer.IsPartOfCombination(upcomingText))
                    {
                        e.Handled = false;
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
                else
                {
                    e.Handled = false;
                }
            }
            else
            {
                e.Handled = true;
            }

            if (e.Handled)
            {
                loggerService.LogMessage(this, $"key {caseSensitiveCharacter} suppressed", "Purple");
            }
            else loggerService.LogMessage(this, $"key {caseSensitiveCharacter} passed through", "Green");

            return e.Handled;
        }

        public bool ShouldSkipKey(string keyName)
        {
            keyName = keyName.ToLower();

            TransliterationTableModel tableModel = liveTransliterator.ukrTranslit.transliterationTableModel;

            // this condition checks for keys that are not in the alphabet (such as shift, ctrl, alt, etc.) and should be skipped
            if (!tableModel.alphabet.Contains(keyName))
            {
                return true;
            }

            // this condition checks for combinations such as ctrl + key, alt + key, win + key (those should be skipped), except for shift + key (that's capitalization and is of interest to us)
            return !keyStateChecker.IsShiftPressedDown() && keyStateChecker.IsModifierPressedDown();
        }

        public bool ShouldClearMemory()
        {
            // checks for alt + tab combo
            return keyStateChecker.IsKeyDown(Key.Tab) && (keyStateChecker.IsKeyDown(Key.LeftAlt) || keyStateChecker.IsKeyDown(Key.RightAlt));
        }

        public void HandleBackspace()
        {
            if (memory.Count == 0)
            {
                return;
            }
            // ctrl + backspace erases entire word and needs additional handling. Here word = any sequence of characters such as abcdefg123, but not punctuation or other special symbols
            if (keyStateChecker.IsKeyDown(Key.LeftCtrl) || keyStateChecker.IsKeyDown(Key.RightCtrl))
            {
                // erase untill apostrophe is met
                while (memory.Count > 0 && memory.Last() != "'")
                {
                    memory.RemoveAt(memory.Count - 1);
                }
            }
            else
            {
                memory.RemoveAt(memory.Count - 1);
            }
        }
    }
}