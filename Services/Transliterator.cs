using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TransliteratorWPF_Version.Helpers;
using TransliteratorWPF_Version.Models;

namespace TransliteratorWPF_Version.Services
{
    public class TransliteratorService : BaseTransliterator, INotifyPropertyChanged
    {
        private readonly SettingsService settingsService;
        public TableKeyAnalyzerService tableKeyAnalayzerService;

        private KeyStateChecker keyStateChecker = new KeyStateChecker();

        private List<string> _translitTables;

        private int _selectedTranslitTableIndex;

        public event PropertyChangedEventHandler PropertyChanged;

        public event Action TransliterationTableChangedEvent;

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
                SetTableModel(TranslitTables[_selectedTranslitTableIndex]);
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
                // Clamps index when a table is deleted from array, just in case the index was pointing to last (now nonexisting) element
                if (TranslitTables != null && value.Count < _translitTables.Count)
                {
                    SelectedTranslitTableIndex = Math.Clamp(SelectedTranslitTableIndex, 0, value.Count);
                }

                _translitTables = value;

                NotifyPropertyChanged();
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static TransliteratorService _instance;

        public static TransliteratorService GetInstance()
        {
            _instance ??= new TransliteratorService();
            return _instance;
        }

        private TransliteratorService()
        {
            // TODO: Dependency injection

            settingsService = SettingsService.GetInstance();

            // warning: hardcoded
            TranslitTables = Utilities.getAllFilesInFolder(@"Resources/TranslitTables").ToList();
            string lastTranslitTable = settingsService.LastSelectedTranslitTable;
            int lastTranslitTableIndex = TranslitTables.FindIndex((tableString) => tableString == lastTranslitTable);
            SelectedTranslitTableIndex = Math.Clamp(lastTranslitTableIndex, 0, TranslitTables.Count - 1);

            SetTableModel(TranslitTables[SelectedTranslitTableIndex]);

            tableKeyAnalayzerService = new TableKeyAnalyzerService(transliterationTableModel.combos);
        }

        public new void SetTableModel(string relativePathToJsonFile)
        {
            base.SetTableModel(relativePathToJsonFile);

            TransliterationTableChangedEvent?.Invoke();

            tableKeyAnalayzerService = new TableKeyAnalyzerService(transliterationTableModel.combos);
        }

        override public string GetCaseForNonalphabeticString(string replacement)
        {
            if (keyStateChecker.IsLowerCase()) return replacement.ToLower();
            return replacement.ToUpper();
        }
    }
}