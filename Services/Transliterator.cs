using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using TransliteratorWPF_Version.Helpers;
using TransliteratorWPF_Version.Models;
using TransliteratorWPF_Version.Views;
using Application = System.Windows.Application;

namespace TransliteratorWPF_Version.Services
{
    public class Transliterator : INotifyPropertyChanged
    {
        private readonly SettingsService settingsService;
        public TableKeyAnalyzerService tableKeyAnalayzerService;

        // TODO: Annotate
        private Dictionary<string, string> PostreplacementMap = new Dictionary<string, string>()
        {
            {  "̚̚̚̚ϟ", "'"}
        };

        private List<string> _translitTables;

        private int _selectedTranslitTableIndex;

        // TODO: Refactor?
        public int SelectedTranslitTableIndex
        {
            get
            {
                return _selectedTranslitTableIndex;
            }
            set
            {
                _selectedTranslitTableIndex = Math.Clamp(value, 0, TranslitTables.Count - 1);
                SetReplacementMap(TranslitTables[_selectedTranslitTableIndex]);
                NotifyPropertyChanged();
            }
        }

        // TODO: Refactor?
        public List<string> TranslitTables
        {
            get
            {
                return _translitTables;
            }

            set
            {
                // TODO: Annotate
                if (TranslitTables != null && value.Count < _translitTables.Count)
                {
                    SelectedTranslitTableIndex = _translitTables.Count - 1;
                }

                _translitTables = value;

                NotifyPropertyChanged();
            }
        }

        public TransliterationTableModel transliterationTableModel;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event Action TransliterationTableChangedEvent;

        public List<string> wordendersVanilla;

        public Transliterator()
        {
            // TODO: Dependency injection

            settingsService = SettingsService.GetInstance();

            // warning: hardcoded
            TranslitTables = Utilities.getAllFilesInFolder(@"Resources/TranslitTables").ToList();
            string lastTranslitTable = settingsService.LastSelectedTranslitTable;
            int lastTranslitTableIndex = TranslitTables.FindIndex((tableString) => tableString == lastTranslitTable);
            SelectedTranslitTableIndex = Math.Clamp(lastTranslitTableIndex, 0, TranslitTables.Count - 1);

            SetReplacementMap(TranslitTables[SelectedTranslitTableIndex]);

            // TODO: Decouple TableKeyAnalyzerSerivce from Transliterator
            tableKeyAnalayzerService = new TableKeyAnalyzerService(transliterationTableModel.combos);
        }

        // TODO: Decouple. Perhaps through inheritance?
        private KeyStateChecker keyStateChecker = new KeyStateChecker();

        public void SetReplacementMap(string relativePathToJsonFile)
        {
            transliterationTableModel = new TransliterationTableModel(relativePathToJsonFile);
            TransliterationTableChangedEvent?.Invoke();

            tableKeyAnalayzerService = new TableKeyAnalyzerService(transliterationTableModel.combos);
        }

        public string Transliterate(string text)
        {
            // TODO: explain why combinations should be transliterated first
            // loop over all possible user inputs and replace them with corresponding transliterations.
            // Replacement map is sorted, thus combinations will be transliterated first

            foreach (string key in transliterationTableModel.keys)
            {
                text = ReplaceKeepCase(key, transliterationTableModel.replacementTable[key], text);

                if (IsNonASCII(text))
                {
                    continue;
                }
            }

            return Postprocess(text);
        }

        public bool IsNonASCII(string text)
        {
            // does C# have .isASCII ?
            // here some simple solution to this:
            // https://stackoverflow.com/questions/18596245/in-c-how-can-i-detect-if-a-character-is-a-non-ascii-character

            if (text.Any((char_) =>
            {
                return char_ >= 128;
            })) return true;
            return false;
        }

        // how to read the param list:
        // replace all "words" in "text" with "replacement"
        public string ReplaceKeepCase(string word, string replacement, string text)
        {
            Func<Match, string> onMatch = match =>
            {
                string matchString = match.Value;

                // nonalphabetic characters don't have uppercase
                // we can optimize this part by making a dictionary for such characters when replacement map is installed
                if (!Utilities.HasUpperCase(matchString))
                {
                    if (keyStateChecker.IsLowerCase()) return replacement.ToLower();
                    return replacement.ToUpper();
                }

                if (Utilities.IsLowerCase(matchString)) return replacement.ToLower();
                if (char.IsUpper(matchString[0])) return replacement.ToUpper(); // now what if the replacement consists of several letters?
                // if last character is uppercase, replacement should be uppercase as well
                if (char.IsUpper(matchString[matchString.Length - 1])) return replacement.ToUpper();
                // not sure if C# has a method for converting to titlecase
                if (Utilities.IsUpperCase(matchString)) return replacement.ToUpper();

                return replacement;
            };

            return Regex.Replace(text, Regex.Escape(word), new MatchEvaluator(onMatch), RegexOptions.IgnoreCase);
        }

        // TODO: Annotate
        public string Postprocess(string text)
        {
            foreach (string char_ in PostreplacementMap.Keys)
            {
                text = text.Replace(char_, PostreplacementMap[char_]);
            }

            return text;
        }
    }
}