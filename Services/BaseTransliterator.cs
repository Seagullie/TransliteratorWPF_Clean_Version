using System;
using System.Text.RegularExpressions;
using TransliteratorWPF_Version.Helpers;
using TransliteratorWPF_Version.Models;

// Is it even a service? Send help
namespace TransliteratorWPF_Version.Services
{
    public class BaseTransliterator
    {
        public TransliterationTableModel transliterationTableModel = null;

        protected BaseTransliterator()
        {
        }

        public void SetTableModel(string relativePathToJsonFile)
        {
            transliterationTableModel = new TransliterationTableModel(relativePathToJsonFile);
        }

        public string Transliterate(string text = "")
        {
            if (transliterationTableModel == null)
            {
                throw new Exception(".transliterationTableModel is not initialized");
            }

            // table keys and inputlText should have same case
            string inputText = text.ToLower();
            string transliteratedText = text;

            // loop over all possible user inputs and replace them with corresponding transliterations.
            // Replacement map is sorted, thus combinations will be transliterated first
            // This is because individual characters that make up a combination may also be present as separate replacements in the transliterationTableModel.replacementTable. By transliterating combinations first, the separate characters that make up the combination will not be replaced individually and will instead be treated as a single unit.
            // © ChatGPT

            foreach (string key in transliterationTableModel.keys)
            {
                // skip keys not present in the text
                if (!inputText.Contains(key))
                {
                    continue;
                }

                transliteratedText = ReplaceKeepCase(key, transliterationTableModel.replacementTable[key], transliteratedText);
                // remove already transliterated keys from inputlText. This is needed to prevent some bugs
                inputText = inputText.Replace(key, "");
            }

            return transliteratedText;
        }

        // how to read the param list:
        // replace all "words" in "text" with "replacement"
        public string ReplaceKeepCase(string word, string replacement, string text)
        {
            Func<Match, string> onMatch = match =>
            {
                string matchString = match.Value;

                // nonalphabetic characters don't have uppercase
                // TODO: Optimize this part by making a dictionary for such characters when replacement map is installed
                if (!Utilities.HasUpperCase(matchString))
                {
                    return GetCaseForNonalphabeticString(replacement);
                }

                if (Utilities.IsLowerCase(matchString)) return replacement.ToLower();
                if (char.IsUpper(matchString[0])) return replacement.ToUpper();
                // ^TODO: handle case when the replacement consists of several letters

                // if last character is uppercase, replacement should be uppercase as well:
                if (char.IsUpper(matchString[matchString.Length - 1])) return replacement.ToUpper();
                // not sure if C# has a method for converting to titlecase
                if (Utilities.IsUpperCase(matchString)) return replacement.ToUpper();

                return replacement;
            };

            return Regex.Replace(text, Regex.Escape(word), new MatchEvaluator(onMatch), RegexOptions.IgnoreCase);
        }

        // TODO: Implement
        virtual public string GetCaseForNonalphabeticString(string replacement)
        {
            return replacement;
        }
    }
}