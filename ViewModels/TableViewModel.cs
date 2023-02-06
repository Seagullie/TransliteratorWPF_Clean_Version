using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransliteratorWPF_Version.Services;

namespace TransliteratorWPF_Version.ViewModels
{
    public partial class TableViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<string> translitTables;

        [ObservableProperty]
        private string selectedTableName;

        public TransliteratorService transliteratorService;

        public TableViewModel()
        {
            transliteratorService = TransliteratorService.GetInstance();

            TranslitTables = transliteratorService.TranslitTables;
            SelectedTableName = transliteratorService.transliterationTableModel.replacementMapFilename;

            transliteratorService.TransliterationTableChangedEvent += () => SelectedTableName = transliteratorService.transliterationTableModel.replacementMapFilename;

            // TODO: Subscribe to TransliterationTablesListChangedEvent
        }

        [RelayCommand]
        private void ToggleAppState()
        {
        }
    }
}