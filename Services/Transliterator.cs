using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using TransliteratorWPF_Version.Helpers;
using TransliteratorWPF_Version.Views;
using Application = System.Windows.Application;

namespace TransliteratorWPF_Version.Services
{
    public class Transliterator : INotifyPropertyChanged
    {
        public HashSet<string> alphabet = new HashSet<string> { };

        private readonly SettingsService settingsService;
        public TableKeyAnalyzerService tableKeyAnalayzerService;

        private Dictionary<string, string> PostreplacementMap = new Dictionary<string, string>()
        {
            {  "̚̚̚̚ϟ", "'"}
        };

        private List<string> _translitTables;

        private int _selectedTranslitTableIndex;

        public int selectedTranslitTableIndex
        {
            get
            {
                return _selectedTranslitTableIndex;
            }
            set
            {
                _selectedTranslitTableIndex = value >= 0 && value < TranslitTables.Count ? value : 0;
                ReadReplacementMapFromJson(TranslitTables[_selectedTranslitTableIndex]);
                NotifyPropertyChanged();
            }
        }

        public List<string> TranslitTables
        {
            get
            {
                return _translitTables;
            }

            set
            {
                if (TranslitTables != null && value.Count < _translitTables.Count)
                {
                    selectedTranslitTableIndex = _translitTables.Count - 1;
                }

                _translitTables = value;

                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // those are the symbols that never occur somewhere within regular words, but rather at the end
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

        public List<string> wordendersVanilla;

        public Transliterator()
        {
            // TODO: Dependency injection

            // .toList() returns a copy of existing array
            // TODO: explain wordendersVanilla or rename the variable to something more descriptive
            wordendersVanilla = wordenders.ToList();
            settingsService = SettingsService.GetInstance();

            // warning: hardcoded
            TranslitTables = Utilities.getAllFilesInFolder(@"Resources/TranslitTables").ToList();
            string lastTranslitTable = settingsService.LastSelectedTranslitTable;
            int lastTranslitTableIndex = TranslitTables.FindIndex((tableString) => tableString == lastTranslitTable);
            // TODO: rewrite clamp
            selectedTranslitTableIndex = Math.Clamp(lastTranslitTableIndex, 0, 99999);

            SetReplacementMapFromJson(TranslitTables[selectedTranslitTableIndex]);

            // TODO: Decouple TableKeyAnalyzerSerivce from Transliterator
            tableKeyAnalayzerService = new TableKeyAnalyzerService(combos);
        }

        private string[] keys;

        public Dictionary<string, string> ReplacementTable = new Dictionary<string, string>();

        // combo = more than one letter. Examples of combos: ch, sh, zh
        // while s d f are not combos
        private string[] combos;

        private KeyStateChecker keyStateChecker = new KeyStateChecker();
        public string replacementMapFilename;

        public void SetReplacementMap(Dictionary<string, string> replacementTable)
        {
            ReplacementTable = replacementTable;

            keys = SortReplacementMapKeys(new Dictionary<string, string>(replacementTable));

            combos = GetReplacementMapCombos(this.ReplacementTable);

            tableKeyAnalayzerService = new TableKeyAnalyzerService(combos);

            // TODO: rewrite explanation
            // generating alphabet:
            // alphabet is a set of all letters that can be transliterated
            foreach (string key in keys)
            {
                if (key.Length != 1)
                {
                    foreach (char subkey in key)
                    {
                        alphabet.Add(subkey.ToString());
                    }
                }
                else
                {
                    alphabet.Add(key);
                }
            }

            // excluding alphabet keys from wordenders

            wordenders = wordendersVanilla;

            foreach (string alphabetLetter in alphabet)
            {
                if (wordenders.Contains(alphabetLetter))
                {
                    wordenders.Remove(alphabetLetter);
                }
            }
        }

        public string[] GetReplacementMapCombos(Dictionary<string, string> replacement_map)
        {
            string[] keys = replacement_map.Keys.ToArray();
            string[] combos = keys.Where(key => key.Length > 1).ToArray();

            return combos;
        }

        // sorting is necessary so that the longest combos are moved to top
        public string[] SortReplacementMapKeys(Dictionary<string, string> replacement_map)
        {
            string[] keys = replacement_map.Keys.ToArray();

            keys = keys.OrderBy(key => key.Length).ToArray();

            Array.Reverse(keys);

            return keys;
        }

        public void SetReplacementMapFromJson(string relativePathToJsonFile) // is it relative or not? relative. Just the name of .json is enough
        {
            try
            {
                Dictionary<string, string> TableAsDictionary = ReadReplacementMapFromJson(relativePathToJsonFile);

                SetReplacementMap(TableAsDictionary);
            }
            finally
            {
                replacementMapFilename = relativePathToJsonFile;
            }
        }

        public Dictionary<string, string> ReadReplacementMapFromJson(string pathToJsonFile)
        {
            string TableAsString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Resources\\TranslitTables\\{pathToJsonFile}"));
            dynamic deserializedTableObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(TableAsString);
            Dictionary<string, string> TableAsDictionary = deserializedTableObj;

            return TableAsDictionary;
        }

        public string Transliterate(string text)
        {
            // TODO: explain why combinations should be transliterated first
            // loop over all possible user inputs and replace them with corresponding transliterations.
            // Replacement map is sorted, thus combinations will be transliterated first

            foreach (string key in keys)
            {
                text = ReplaceKeepCase(key, ReplacementTable[key], text);

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
                if (!HasUpperCase(matchString))
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

        // many characters do not have an uppercase version. For example, "!", "?" ":" can't be uppercased, while letters such as a(A), b(B), c(C) do indeed have uppercase variant
        public bool HasUpperCase(string character)
        {
            return character.ToLower() != character.ToUpper();
        }

        // TODO: annotate
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