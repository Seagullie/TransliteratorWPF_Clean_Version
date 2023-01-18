using System;
using System.Text.RegularExpressions;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using Window = System.Windows.Window;

namespace TransliteratorWPF_Version.Views
{
    /// <summary>
    /// Interaction logic for NewTableNameDialog.xaml
    /// </summary>
    public partial class NewTableNameDialog : Window
    {
        public NewTableNameDialog()
        {
            InitializeComponent();
        }

        private bool verifyTableName(string tableName)
        {
            // get win file regex and use it here

            string regex = @"^[\w\-. ]+$";
            Regex winNameRegexp = new Regex(regex,
         RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (Regex.IsMatch(tableName, regex))
            {
                return true;
            }

            return false;
        }

        private void confirmBtn_Click(object sender, RoutedEventArgs e)
        {
            // verify table name. If verification fails, display infoBox window with notification about the error and return

            if (!verifyTableName(textBox1.Text))
            {
                string messageBoxText = "The name should contain only alphanumeric characters, -, _, . and space";
                string caption = "Warning";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBoxResult result;

                result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);

                //Help.ShowPopup(textBox1, "The name should contain only alphanumeric characters, -, _, . and space", new Point(this.Left + textBox1.Margin.Left, this.Top + textBox1.Margin.Top));

                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            void action() => this.InvalidateMeasure();
            this.Dispatcher.BeginInvoke((Action)action);
        }
    }
}