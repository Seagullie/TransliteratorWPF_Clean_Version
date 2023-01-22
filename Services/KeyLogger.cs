using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using TransliteratorWPF_Version.Helpers;
using TransliteratorWPF_Version.Properties;
using TransliteratorWPF_Version.Views;
using Application = System.Windows.Application;
using Key = System.Windows.Input.Key;

namespace TransliteratorWPF_Version.Services
{
    public sealed partial class KeyLogger : ObservableObject
    {
        [ObservableProperty]
        public bool state = true;

        public string alphabet;

        [ObservableProperty]
        public string stateDesc = "On";

        public LiveTransliterator liveTransliterator;

        public event EventHandler StateChanged;
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

        public List<string> memory = new List<string>();

        public KeyStateChecker keyStateChecker = new KeyStateChecker();

        public List<string> keysToIgnore = new List<string> { };

        public GlobalKeyboardHook gkh;

        [ObservableProperty]
        public HashSet<string> toggleTranslitShortcut = new HashSet<string>();

        public HashSet<string> ChangeLanguageShortcut = new HashSet<string> { "Alt", "LShiftKey" };

        public HashSet<string> ChangeLanguageShortcut3 = new HashSet<string> { "LMenu", "Shift" };

        public string ToggleTranslitShortcutMainKey;
        public int nOfKeysOmmittedFromMemory = 0;

        private KeyLogger()
        {
            gkh = new GlobalKeyboardHook();

            fetchToggleTranslitShortcutFromSettings();
            gkh_KeyDownHandler = new KeyEventHandler(gkh_KeyDown);
        }

        private static KeyLogger _instance;

        public static KeyLogger GetInstance()
        {
            if (_instance == null)
            {
                _instance = new KeyLogger();
            }
            return _instance;
        }

        public void fetchToggleTranslitShortcutFromSettings()
        {
            string SettingsString = Settings.Default.ToggleTranslitShortcut;
            string[] SettingStringAsArray;
            if (SettingsString.Contains("+"))
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

        public void LogKeys()
        {
            foreach (int value in Enumerable.Range(65, 25 + 1))
            {
                gkh.HookedKeys.Add((Keys)value);
            }

            foreach (int value in Enumerable.Range(48, 10))
            {
                gkh.HookedKeys.Add((Keys)value);
            }

            gkh.HookedKeys.Add(Keys.Back);

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
            ToUnicodeEx(virtualKeyCode, scanCode, keyboardState, result, 5, 0, inputLocaleIdentifier);

            return result.ToString();
        }

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

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

            debugWindow?.ConsoleLog($"ALT down: {altDown}. KeyString: {keyString}");

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
            else if (Settings.Default.suppressAltShift && (registeredKeyStrokesAsHash.SetEquals(ChangeLanguageShortcut) || registeredKeyStrokesAsHash.SetEquals(ChangeLanguageShortcut3)))
            {
                e.Handled = true;
                return true;
            }

            return false;
        }

        private void gkh_KeyDown(object sender, KeyEventArgs e)
        {
            debugWindow?.ConsoleLog("key pressed");

            if (HandleShortcuts(e)) return;

            if (State == false)
            {
                debugWindow?.ConsoleLog("Translit off. Key skipped");
                return;
            }

            var UnicodeChar = KeyCodeToUnicode(e.KeyCode);

            if (UnicodeChar.Length == 0) return;

            bool shouldIgnoreKey = (debugWindow?.underTestByWinDriverCheckBox.IsChecked == true || gkh.alwaysAllowInjected || gkh.skipInjected != true) && keysToIgnore.Count > 0 && keysToIgnore[0] == UnicodeChar;

            if (shouldIgnoreKey)
            {
                keysToIgnore.RemoveAt(0);
                debugWindow?.ConsoleLog($"ignoring this key: {UnicodeChar}");
                return;
            }

            debugWindow?.ConsoleLog($"{e.KeyCode} (Unicode: {UnicodeChar}) DOWN");

            if (ShouldClearMemory())
            {
                memory.Clear();
                return;
            }

            if (e.KeyCode.ToString() == "Back")
            {
                HandleBackspace();
                debugWindow?.ConsoleLog($"Handling {e.KeyCode}");
                return;
            }

            string lowerCaseKeyCode = e.KeyCode.ToString().ToLower();

            if (ShouldSkipKey(UnicodeChar) && !isWordender(lowerCaseKeyCode, UnicodeChar))
            {
                debugWindow?.ConsoleLog($"Skipping {e.KeyCode}");
                return;
            }

            if (memory.Count > 20) memory.Clear();

            bool isLowerCase = keyStateChecker.IsLowerCase();
            string caseSensitiveCharacter = isLowerCase ? UnicodeChar.ToLower() : UnicodeChar.ToUpper();
            debugWindow?.ConsoleLog($"Keycode: {caseSensitiveCharacter}. KeyData: {e.KeyData} DOWN.");

            if (isWordender(lowerCaseKeyCode, UnicodeChar))
            {
                handleWordenderKey(UnicodeChar, ref e);
            }
            else
            {
                if (liveTransliterator.displayCombos)
                {
                    decideOnKeySuppression(caseSensitiveCharacter.ToLower(), ref e);

                    ref var translit = ref (liveTransliterator.ukrTranslit);
                    string upcomingText = (GetMemoryAsString() + caseSensitiveCharacter).ToLower();

                    if (translit.EndsWithBrokenCombo(upcomingText) && translit.EndsWithComboInit(upcomingText) && !translit.IsPartOfCombination(upcomingText))
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
                else
                {
                    e.Handled = true;

                    memory.Add(caseSensitiveCharacter);
                    liveTransliterator.Transliterate(caseSensitiveCharacter);
                }
            }
        }

        public bool isWordender(string lowerCaseKeyCode, string UnicodeChar)
        {
            var wordenders = liveTransliterator.ukrTranslit.wordenders;

            return wordenders.Contains(lowerCaseKeyCode) || wordenders.Contains(UnicodeChar.ToLower());
        }

        public void handleWordenderKey(string UnicodeChar, ref KeyEventArgs e)
        {
            if (memory.Count == 0)
            {
                return;
            }

            e.Handled = true;

            liveTransliterator.Transliterate(UnicodeChar, true);
            liveTransliterator.Write(UnicodeChar);
        }

        public bool decideOnKeySuppression(string caseSensitiveCharacter, ref KeyEventArgs e)
        {
            ref Transliterator translit = ref liveTransliterator.ukrTranslit;

            if (translit.IsPartOfCombination(caseSensitiveCharacter))
            {
                string upcomingText = (GetMemoryAsString() + caseSensitiveCharacter).ToLower();

                if (translit.isSubComboFinisher(caseSensitiveCharacter))
                {
                    e.Handled = false;
                }
                else if (translit.isCombo(upcomingText))
                {
                    e.Handled = true;
                    debugWindow?.ConsoleLog($"{caseSensitiveCharacter} – is full combo finisher");
                }
                else if (translit.EndsWithBrokenCombo(upcomingText) && !translit.IsPartOfCombination(upcomingText))
                {
                    e.Handled = true;
                }
                else if (!translit.IsStartOfCombination(caseSensitiveCharacter))
                {
                    if (upcomingText.Length > 1 && translit.IsPartOfCombination(upcomingText))
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
                debugWindow?.ConsoleLog($"key {caseSensitiveCharacter} suppressed", "Purple");
            }
            else debugWindow?.ConsoleLog($"key {caseSensitiveCharacter} passed through", "Green");

            return e.Handled;
        }

        public void LogKey(object e)
        {
            Console.WriteLine("logging a key");
        }

        public bool ShouldSkipKey(string key_name)
        {
            key_name = key_name.ToLower();

            if (!liveTransliterator.ukrTranslit.alphabet.Contains(key_name))
            {
                return true;
            }

            if (!keyStateChecker.IsShiftPressedDown())
            {
                if (keyStateChecker.IsModifierPressedDown())
                {
                    return true;
                }
            }

            return false;
        }

        public bool ShouldClearMemory()
        {
            if (keyStateChecker.IsKeyDown(Key.Tab) && (keyStateChecker.IsKeyDown(Key.LeftAlt) || keyStateChecker.IsKeyDown(Key.RightAlt)))
            {
                return true;
            }

            return false;
        }

        public void HandleBackspace()
        {
            if (memory.Count == 0)
            {
                return;
            }
            if (keyStateChecker.IsKeyDown(Key.LeftCtrl) || keyStateChecker.IsKeyDown(Key.RightCtrl))
            {
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

        public void Pause()
        {
            State = false;
            StateDesc = "Off";

            memory.Clear();
        }

        public void Cont()
        {
            State = true;        
            StateDesc = "On";
        }

        public void Toggle()
        {
            if (State == true)
            {
                Pause();
            }
            else
            {
                Cont();
            }
        }

        partial void OnStateChanged(bool value)
        {
            StateChanged.Invoke(this, EventArgs.Empty);
        }
    }
}