using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TransliteratorWPF_Version.ViewModels;

namespace TransliteratorWPF_Version.Views
{
    /// <summary>
    /// Interaction logic for SnippetTranslitWindow.xaml
    /// </summary>
    public partial class SnippetTranslitWindow : Window
    {

        public SnippetTranslitViewModel ViewModel { get; private set; }


        public SnippetTranslitWindow()
        {
            InitializeComponent();
            ViewModel = new();
            DataContext = ViewModel;
        }


        // make window draggable
        private void SnippetTranslitWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}