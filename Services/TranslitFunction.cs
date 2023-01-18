using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using TransliteratorWPF_Version.Helpers;
using TransliteratorWPF_Version.Properties;
using TransliteratorWPF_Version.Views;
using Application = System.Windows.Application;

// I want to write the tables as json files and then import those files here.
// But it is important to have them easily accessible and editable.
// And, apparently, the json files that I include as resources are embedded into exe?
// Or into some other file. Or perhaps I'm not building it right?
// Perhaps you setup everything with an installer? https://stackoverflow.com/questions/2251062/how-to-make-an-installer-for-my-c-sharp-application
namespace TransliteratorWPF_Version.Services
{
    public class TranslitFunction
    {
        private TranslitFunction(string[] args)
        {
            var translit = new Transliterator();
            var translitResult = translit.transliterate("schuka");

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine(translitResult);
        }
    }

    // trying to find a way to surround text with braces by selecting it and pressing the key for opening brace
    // reading this SO thread and trying things out, but to no avail
    // trying out AutoSurround plugin. It works.

    public class Transliterator : INotifyPropertyChanged
    {
        public HashSet<string> alphabet = new HashSet<string> { };

        public App app
        {
            get
            {
                //ref var currentApp = ref Application.Current;
                return (App)Application.Current;
            }
        }

        private Dictionary<string, string> postreplacement_map = new Dictionary<string, string>()
        {
            {  "̚̚̚̚ϟ", "'"}
        };

        //private DebugWindow debugWindow = System.Windows.Application.Current.Windows.;

        //private DebugWindow _debugWindow;

        //private List<string> _translitTables;
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
                _selectedTranslitTableIndex = value >= 0 && value < translitTables.Count ? value : 0;
                ReadReplacementMapFromJson(translitTables[_selectedTranslitTableIndex]);
                NotifyPropertyChanged();
            }
        }

        public List<string> translitTables
        {
            get
            {
                return _translitTables;
            }

            set
            {
                // should make a test around this case:
                // a new table is created. it's added as a last entry due to sorting.
                // the new table is selected as main in transliterator.
                // the new table is deleted
                // main table should update to the last available table

                if (translitTables != null && value.Count < _translitTables.Count)
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
            // .toList() returns a copy of existing array
            wordendersVanilla = wordenders.ToList();

            translitTables = app.getAllTranslitTableNames().ToList();
            string lastTranslitTable = Settings.Default.lastTranslitTable;
            int lastTranslitTableIndex = translitTables.FindIndex((tableString) => tableString == lastTranslitTable);
            if (lastTranslitTableIndex != -1)
            {
                selectedTranslitTableIndex = lastTranslitTableIndex;
            }
            else
            {
                selectedTranslitTableIndex = 0;
            }

            SetReplacementMapFromJson(translitTables[selectedTranslitTableIndex]);
        }

        public DebugWindow debugWindow
        {
            get
            {
                WindowCollection windows = Application.Current.Windows;
                foreach (Window window in windows)
                {
                    // warning: hardcoded
                    if (window.Name == "DebugWindow1")
                    {
                        return (DebugWindow)window;
                    }
                }

                return null;
                //return windows
            }
        }

        private string[] keys;

        public Dictionary<string, string> replacement_map = new Dictionary<string, string>();
        //{
        //    { "sch", "щ" },
        //    { "sh", "ш" },
        //    { "ch", "ч" },
        //    { "zh", "ж" },
        //    { "ju", "ю" },
        //    { "ja", "я" },
        //    { "je", "є" },
        //    { "ji", "ї" },

        //    { "''", "̚̚̚̚ϟ" },

        //    { "w", "в" },
        //    { "e", "е" },
        //    { "r", "р" },
        //    { "t", "т" },
        //    { "y", "и" },
        //    { "u", "у" },
        //    { "i", "і" },
        //    { "o", "о" },
        //    { "p", "п" },
        //    { "a", "а" },
        //    { "s", "с" },
        //    { "d", "д" },
        //    { "f", "ф" },
        //    { "g", "г" },
        //    { "h", "г" },
        //    { "j", "й" },
        //    { "k", "к" },
        //    { "l", "л" },
        //    { "z", "з" },
        //    { "x", "х" },
        //    { "c", "ц" },
        //    { "v", "в" },
        //    { "b", "б" },
        //    { "n", "н" },
        //    { "m", "м" },
        //    { "'", "ь" }
        //};

        private string[] combos;
        private KeyStateChecker keyStateChecker = new KeyStateChecker();
        public string replacement_map_filename;

        public void SetReplacementMap(Dictionary<string, string> replacement_map)
        {
            this.replacement_map = replacement_map;

            keys = SortReplacementMapKeys(new Dictionary<string, string>(replacement_map));

            combos = getReplacementMapCombos(this.replacement_map);

            // generating alphabet:

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

        public string[] getReplacementMapCombos(Dictionary<string, string> replacement_map)
        {
            string[] keys = replacement_map.Keys.ToArray();
            string[] combos = keys.Where(key => key.Length > 1).ToArray();

            return combos;
        }

        public string[] SortReplacementMapKeys(Dictionary<string, string> replacement_map)
        {
            string[] keys = replacement_map.Keys.ToArray();

            keys = keys.OrderBy(key => key.Length).ToArray();

            Array.Reverse(keys);

            return keys;
        }

        public SortedDictionary<string, string> SortReplacementMapByKeyLength(Dictionary<string, string> replacement_map)
        {
            string[] sortedKeys = SortReplacementMapKeys(replacement_map);

            SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();

            foreach (string key in sortedKeys)
            {
                sortedDictionary[key] = replacement_map[key];
            }

            return sortedDictionary;
        }

        public void SetReplacementMapFromJson(string pathToJsonFile) // is it relative or not? relative. Just the name of .json is enough
        {
            try
            {
                Dictionary<string, string> TableAsDictionary = ReadReplacementMapFromJson(pathToJsonFile);

                SetReplacementMap(TableAsDictionary);
            }
            finally
            {
                replacement_map_filename = pathToJsonFile;
            }
        }

        public Dictionary<string, string> ReadReplacementMapFromJson(string pathToJsonFile)
        {
            string TableAsString = File.ReadAllText(Path.Combine(App.BaseDir, $"Resources\\translitTables\\{pathToJsonFile}"));
            dynamic deserializedTableObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(TableAsString);
            Dictionary<string, string> TableAsDictionary = deserializedTableObj;

            return TableAsDictionary;
        }

        public string transliterate(string text)
        {
            foreach (string key in keys)
            {
                text = ReplaceKeepCase(key, replacement_map[key], text);

                if (is_nonASCII(text))
                {
                    continue;
                }
            }

            return postprocess(text);
        }

        public bool is_nonASCII(string text)
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

        public string ReplaceKeepCase(string word, string replacement, string text)
        {
            // how to read the param list:
            // replace all "words" in "text" with "replacement"

            // getting "namespace cannot directly contain fields or methods"
            // that was due to me accidentaly declaring methods outside the class

            Func<Match, string> onMatch = match =>
            {
                string val = match.Value;

                // nonalphabetic characters don't have uppercase
                if (!HasUpperCase(val))
                {
                    // we can optimize this part by making a dictionary for such characters when replacement map is installed

                    if (keyStateChecker.isLowerCase()) return replacement.ToLower();
                    return replacement.ToUpper();
                }

                // how do I check whether a string is in lowercase... Is there a method for that?
                if (val.All(char.IsLower)) return replacement.ToLower();
                if (char.IsUpper(val[0])) return replacement.ToUpper(); // now what if the replacement consists of several letters?
                // if last character is uppercase, replacement should be uppercase as well
                if (char.IsUpper(val[val.Length - 1])) return replacement.ToUpper();
                // not sure if C# has a method for converting to titlecase
                if (val.All(char.IsUpper)) return replacement.ToUpper();

                return replacement;
            };

            //Regex.Replace(word, onMatch, text, flags = re.I);
            return Regex.Replace(text, Regex.Escape(word), new MatchEvaluator(onMatch), RegexOptions.IgnoreCase);
            //return sub(word, onMatch, text, flags = re.I);
        }

        public bool HasUpperCase(string character)
        {
            return character.ToLower() != character.ToUpper();
        }

        public string postprocess(string text)
        {
            foreach (string char_ in postreplacement_map.Keys)
            {
                text = text.Replace(char_, postreplacement_map[char_]);
            }

            return text;
        }

        public bool IsStartOfCombination(string text)
        {
            text = text.ToLower();

            foreach (string combo in combos)
            {
                if (combo.StartsWith(text) && combo.Length > text.Length) return true;
            };

            return false;
        }

        public bool IsPartOfCombination(string text)
        {
            text = text.ToLower();

            foreach (string combo in combos)
            {
                if (combo.Contains(text) && combo.Length > text.Length) return true;
            };

            return false;
        }

        public bool EndsWithComboInit(string text)
        {
            return IsStartOfCombination(text[text.Length - 1].ToString());
        }

        // aren't .is_part_of_combination and .EndsWithComboInit the same?
        // no, cause .EndsWithComboInit checks last character only, while .is_part_of_combination entire parameter on whether it is a substring of existing combo.

        public bool LastCharacterIsComboInit(string text)
        {
            foreach (string combo in combos)
            {
                if (text.EndsWith(combo)) return true;
            };

            return false;
        }

        public bool isCombo(string text)
        {
            return combos.Contains(text);
        }

        public bool isSubComboFinisher(string character)
        {
            return false; // warning danger. Will implement later
        }

        public bool isAnyComboFinisher(string character)
        {
            foreach (string combo in combos)
            {
                if (combo.EndsWith(character))
                {
                    return true;
                }
            }

            return false;
        }

        public bool isAddingUpToCombo(string prefix, string characterFinisher)
        {
            return isCombo(prefix + characterFinisher);
        }

        public bool EndsWithBrokenCombo(string text)
        {
            // remove last character and see whether the remainder ends with combo init

            if (text.Length < 2)
            {
                return false;
            }

            if (isCombo(text))
            {
                return false;
            }

            string textWithoutLastCharacter = text.Substring(0, text.Length - 1);
            // clamping combo length to text length:
            if (combos.Length == 0)
            {
                return false;
            }
            int longestComboLen = textWithoutLastCharacter.Length < combos[0].Length ? textWithoutLastCharacter.Length : combos[0].Length;

            for (int i = 1; i < longestComboLen + 1; i++)
            {
                // now check all the substrings on whether those are combo inits or not by slicing more and more each time
                string substr = textWithoutLastCharacter.Substring(textWithoutLastCharacter.Length - i);
                if (EndsWithComboInit(substr))
                {
                    debugWindow?.ConsoleLog($"broken combo detected: {text}");
                    return true;
                }
            }

            return false;
        }
    }
}