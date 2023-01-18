using System;
using System.Windows;
using System.Windows.Input;
using TransliteratorWPF_Version.Properties;
using Path = System.IO.Path;

namespace TransliteratorWPF_Version.Views
{
    /// <summary>
    /// Interaction logic for EditToggleSoundsWindow.xaml
    /// </summary>
    public partial class EditToggleSoundsWindow
    {
        public EditToggleSoundsWindow()
        {
            InitializeComponent();
        }

        private void changeSoundOnToggleOffBtn_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "wav_file.wav"; // Default file name
            dialog.DefaultExt = ".wav"; // Default file extension
            dialog.Filter = "Audio documents (.wav)|*.wav"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string pathToFile = dialog.FileName;
                string fileName = Path.GetFileName(pathToFile);
                Settings.Default.pathToCustomToggleOffSound = pathToFile;

                changeSoundOnToggleOffTxtBox.Text = Settings.Default.pathToCustomToggleOffSound;
                changeSoundOnToggleOnTxtBox.Text = Settings.Default.pathToCustomToggleOnSound;

                UpdateTooltips();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            changeSoundOnToggleOffTxtBox.Text = Settings.Default.pathToCustomToggleOffSound;
            changeSoundOnToggleOnTxtBox.Text = Settings.Default.pathToCustomToggleOnSound;

            UpdateTooltips();
        }

        private void UpdateTooltips()
        {
            changeSoundOnToggleOffTxtBox.ToolTip = changeSoundOnToggleOffTxtBox.Text;
            changeSoundOnToggleOnTxtBox.ToolTip = changeSoundOnToggleOnTxtBox.Text;
        }

        private void changeSoundOnToggleOnBtn_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "wav_file.wav"; // Default file name
            dialog.DefaultExt = ".wav"; // Default file extension
            dialog.Filter = "Audio documents (.wav)|*.wav"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string pathToFile = dialog.FileName; // a bit misleading property name
                string fileName = Path.GetFileName(pathToFile);
                Settings.Default.pathToCustomToggleOnSound = pathToFile;

                changeSoundOnToggleOffTxtBox.Text = Settings.Default.pathToCustomToggleOffSound;
                changeSoundOnToggleOnTxtBox.Text = Settings.Default.pathToCustomToggleOnSound;

                UpdateTooltips();
            }
        }

        // remove unpainted white area around borders
        private void Window_Activated(object sender, EventArgs e)
        {
            void action() => this.InvalidateMeasure();
            this.Dispatcher.BeginInvoke((Action)action);
        }

        // make window draggable
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}