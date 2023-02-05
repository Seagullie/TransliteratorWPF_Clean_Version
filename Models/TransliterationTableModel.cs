using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TransliteratorWPF_Version.Models
{
    public class TransliterationTableModel
    {
        public Dictionary<string, string> replacementTable = new();

        public string[] keys;

        // combo = more than one letter. Examples of combos: ch, sh, zh
        // while s d f are not combos
        public string[] combos;

        public HashSet<string> alphabet = new HashSet<string> { };

        public string replacementMapFilename;

        public TransliterationTableModel(string pathToJsonFile)
        {
            replacementMapFilename = pathToJsonFile;
            // TODO: transfer file reading to service
            replacementTable = ReadReplacementMapFromJson(pathToJsonFile);
            keys = SortReplacementMapKeys(new Dictionary<string, string>(replacementTable));
            combos = GetReplacementMapCombos(replacementTable);

            alphabet = ParseAlphabet();
        }

        public Dictionary<string, string> ReadReplacementMapFromJson(string pathToJsonFile)
        {
            string translitTablesDir = $"Resources\\TranslitTables";
            string TableAsString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{translitTablesDir}\\{pathToJsonFile}"));
            dynamic deserializedTableObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(TableAsString);
            Dictionary<string, string> TableAsDictionary = deserializedTableObj;

            return TableAsDictionary;
        }

        // sorting is necessary so that the longest combos are moved to top
        public string[] SortReplacementMapKeys(Dictionary<string, string> replacement_map)
        {
            string[] keys = replacement_map.Keys.ToArray();

            keys = keys.OrderBy(key => key.Length).ToArray();

            Array.Reverse(keys);

            return keys;
        }

        public string[] GetReplacementMapCombos(Dictionary<string, string> replacement_map)
        {
            string[] keys = replacement_map.Keys.ToArray();
            string[] combos = keys.Where(key => key.Length > 1).ToArray();

            return combos;
        }

        // generating alphabet:
        // alphabet is a set of all letters that can be transliterated.
        // basically, all keys from the replacement map + any characters within those keys, if a key consist of more than one character
        private HashSet<string> ParseAlphabet()
        {
            HashSet<string> alphabetHashset = new();

            foreach (string key in keys)
            {
                if (key.Length != 1)
                {
                    foreach (char subkey in key)
                    {
                        alphabetHashset.Add(subkey.ToString());
                    }
                }
                else
                {
                    alphabetHashset.Add(key);
                }
            }

            return alphabetHashset;
        }
    }
}