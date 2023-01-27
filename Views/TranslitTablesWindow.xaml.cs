using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using TransliteratorWPF_Version.Services;
using static TransliteratorWPF_Version.Services.LoggerService;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using TextBox = System.Windows.Controls.TextBox;

namespace TransliteratorWPF_Version.Views
{
    /// <summary>
    /// Interaction logic for TranslitTablesWindow.xaml
    /// </summary>
    public partial class TranslitTablesWindow : Window
    {
        private App app = ((App)Application.Current);
        private readonly LoggerService loggerService;
        private readonly Main liveTransliterator;

        public MainWindow mainWindow
        {
            get
            {
                return (MainWindow)System.Windows.Application.Current.MainWindow;
            }
        }

        public DebugWindow? debugWindow
        {
            get
            {
                WindowCollection windows = System.Windows.Application.Current.Windows;
                foreach (Window window in windows)
                {
                    if (window.Name == "DebugWindow1")
                    {
                        return (DebugWindow)window;
                    }
                }
                return null;
            }
        }

        public TranslitTablesWindow()
        {
            // TODO: Dependency injection
            loggerService = LoggerService.GetInstance();
            liveTransliterator = Main.GetInstance();
            InitializeComponent();
        }

        private void newTableBtn_Click(object sender, RoutedEventArgs e)
        {
            // spawn a new popup with textbox for the name, then create the .json file, reinitialize
            // the combo box and select the new json file
            // you also gotta handle the empty .json file case... pehraps by adding an empty object to your .json file

            NewTableNameDialog dialog = new NewTableNameDialog();

            if (dialog.ShowDialog() == true)
            {
                // Read the contents of testDialog's TextBox.
                string tableName = dialog.textBox1.Text;

                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Resources\TranslitTables\{tableName}.json"), "{}");

                //initializeTablesComboBox(true);
                liveTransliterator.ukrTranslit.TranslitTables.Add($"{tableName}.json");
                // it's sort of like in react. You gotta create a new list, cause the property setter is where update event gets created
                liveTransliterator.ukrTranslit.TranslitTables = liveTransliterator.ukrTranslit.TranslitTables.ToArray().ToList();
            }
            else
            {
                //this.txtResult.Text = "Cancelled";
            }

            //dialog.remove();
        }

        public void initializeTablesComboBox(bool reinitialze = false)
        {
            if (reinitialze)
            {
                comboBox1.Items.Clear();
            }

            string[] tables = app.getAllTranslitTableNames();

            foreach (string table in tables)
            {
                comboBox1.Items.Add(table);
            }
        }

        private void deleteCurrentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                // a notification would be nice here

                loggerService.LogMessage(this, $"A table needs to be selected.");
                return;
            }

            string tableName = comboBox1.SelectedItem.ToString();

            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Resources\TranslitTables\{tableName}"));

            // a notification would be nice here
            loggerService.LogMessage(this, $"{tableName} has been deleted");

            liveTransliterator.ukrTranslit.TranslitTables.Remove(tableName);
            // creating a copy to trigger event
            liveTransliterator.ukrTranslit.TranslitTables = liveTransliterator.ukrTranslit.TranslitTables.ToArray().ToList();

            //initializeTablesComboBox(true);
            //debugWindow?.initializeTablesComboBox(true);
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                return;
            }

            string selectedTranslitTable = comboBox1.SelectedItem.ToString();
            // delete all existing characer fields and generate new ones

            // I would like you to not touch the translit itself
            //mainWindow.liveTranslit.ukrTranslit.SetReplacementMapFromJson($"Resources/translitTables/{selectedTranslitTable}");

            panel1.Children.Clear();

            generateEditBoxesForTranslitTable(liveTransliterator.ukrTranslit.ReadReplacementMapFromJson(selectedTranslitTable));
        }

        // can't see the textboxes at all. Is there a way to inspect all controls available?
        // the same way it is done with browser devtools
        public void generateEditBoxesForTranslitTable(Dictionary<string, string> replacement_map)
        {
            foreach (string key in liveTransliterator.ukrTranslit.SortReplacementMapKeys(replacement_map))
            {
                generatePanelWithEditBoxes(key, replacement_map[key]);
            }

            //CenterWindow();
            // super specific logic here, but I'm too lazy to make it general
            //if (IsWindowOffScreen() != OffScreenDimension.None)
            //{
            //    this.Top = 0;
            //    //this.Left = 0;
            //}
        }

        public enum OffScreenDimension
        {
            Top, Left, Bottom, Right, None
        }

        public Offset IsWindowOffScreen()
        {
            var allScreens = Screen.AllScreens.ToList();
            var thisScreen = allScreens.SingleOrDefault(s => this.Left >= s.WorkingArea.Left && this.Left < s.WorkingArea.Right);

            int top = (int)this.Top;
            int left = (int)this.Left;

            // check whether it's going off to the left or to the top

            if (top < 0)
            {
                return new Offset(OffScreenDimension.Top, top * -1);
            }
            if (left < 0)
            {
                return new Offset(OffScreenDimension.Left, left * -1);
            }

            int bottom = (int)(top + this.ActualHeight);
            int right = (int)(left + this.ActualWidth);

            // check whether it's going off to the right or to the bottom

            int taskBarHeightInPixels = 40;

            if (bottom > (thisScreen.Bounds.Bottom - taskBarHeightInPixels))
            {
                return new Offset(OffScreenDimension.Bottom, bottom - (thisScreen.Bounds.Bottom - taskBarHeightInPixels));
            }

            if (right > thisScreen.Bounds.Right)
            {
                return new Offset(OffScreenDimension.Right, right - thisScreen.Bounds.Right);
            }

            return new Offset(OffScreenDimension.None, 0);
        }

        public void CenterWindow()
        {
            //get the current monitor
            Screen currentMonitor = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow).Handle);

            //find out if our app is being scaled by the monitor
            PresentationSource source = PresentationSource.FromVisual(Application.Current.MainWindow);
            double dpiScaling = (source != null && source.CompositionTarget != null ? source.CompositionTarget.TransformFromDevice.M11 : 1);

            //get the available area of the monitor
            System.Drawing.Rectangle workArea = currentMonitor.WorkingArea;
            var workAreaWidth = (int)Math.Floor(workArea.Width * dpiScaling);
            var workAreaHeight = (int)Math.Floor(workArea.Height * dpiScaling);

            //move to the centre
            Application.Current.MainWindow.Left = (((workAreaWidth - (this.ActualWidth * dpiScaling)) / 2) + (workArea.Left * dpiScaling));
            Application.Current.MainWindow.Top = (((workAreaHeight - (this.ActualHeight * dpiScaling)) / 2) + (workArea.Top * dpiScaling));
        }

        public void generatePanelWithEditBoxes(string key, string value)
        {
            int verticalOffset;

            if (panel1.Children.Count == 0)
            {
                verticalOffset = 25;
            }
            else
            {
                StackPanel lastChild = (StackPanel)panel1.Children[^1];
                verticalOffset = (int)(lastChild.Margin.Top);
            }

            Size textBoxSize = new Size(40, 20);
            int textBoxWidth = 40;
            int textBoxHeight = 20;
            Size hostingPanelSize = new Size((40 + 40) * 2, 20);
            //int hostingPanelWidth = (40 + 40 + 100) * 2;
            int hostingPanelHeight = 20;

            StackPanel hostingPanel = new StackPanel();
            hostingPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            hostingPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            //hostingPanel.Location = new Point(90, verticalOffset + 10);
            hostingPanel.Margin = new Thickness(0, verticalOffset, 0, 0);
            //hostingPanel.Size = hostingPanelSize;
            //hostingPanel.Height = hostingPanelHeight;
            //hostingPanel.Width = hostingPanelWidth;
            //hostingPanel.

            TextBox keyTxtBox = new TextBox();
            //keyTxtBox.Location = new Point(90, verticalOffset + 10);
            keyTxtBox.Text = key;
            //keyTxtBox.Size = textBoxSize;
            //keyTxtBox.Width = textBoxWidth;
            //keyTxtBox.Height = textBoxHeight;
            keyTxtBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            //keyTxtBox.TextAlign = HorizontalAlignment.Center;
            keyTxtBox.TextAlignment = TextAlignment.Center;
            keyTxtBox.Margin = new Thickness(0, 0, 30, 0);
            // struggling with configuring AutoResize property
            //keyTxtBox.Dock = DockStyle.Left;
            //keyTxtBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            TextBox valueTxtBox = new TextBox();
            valueTxtBox.Text = value;
            //valueTxtBox.Location = new Point(90 + 100, verticalOffset + 10);
            //valueTxtBox.Size = textBoxSize;
            //valueTxtBox.Width = textBoxWidth;
            //valueTxtBox.Height = textBoxHeight;

            valueTxtBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            valueTxtBox.TextAlignment = TextAlignment.Center;

            //valueTxtBox.Margin = new Thickness(50);
            //valueTxtBox.Dock = DockStyle.Right;

            hostingPanel.Children.Add(keyTxtBox);
            hostingPanel.Children.Add(valueTxtBox);

            panel1.Children.Add(hostingPanel);
        }

        private void saveTableBtn_Click(object sender, RoutedEventArgs e)
        {
            //ControlCollection transliterationPairs = panel1.Controls;

            Dictionary<string, string> replacementMap = new Dictionary<string, string>();

            foreach (StackPanel panel in panel1.Children)
            {
                string key = ((TextBox)(panel.Children[0])).Text;
                string value = ((TextBox)(panel.Children[1])).Text;

                if (key.Length == 0 || value.Length == 0)
                {
                    continue;
                }

                if (replacementMap.ContainsKey(key))
                {
                    CreateModal("Duplicate keys detected. Delete/Edit them and save again", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                replacementMap[key] = value;
            }

            if (!VerifyCombos(replacementMap))
            {
                return;
            }

            string serializedTable = Newtonsoft.Json.JsonConvert.SerializeObject(replacementMap, Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Resources\\TranslitTables\\{comboBox1.SelectedItem}"), serializedTable);
            // make translit redownload the current table just in case it was updated. Too lazy to check in a more detailed way.

            //string nameOfCurrentlyUsedReplacementMap = mainWindow.translitTablesComboBox.SelectedItem.ToString();
            //liveTransliterator.ukrTranslit.SetReplacementMapFromJson(nameOfCurrentlyUsedReplacementMap);
        }

        //private bool checkForDuplicates(Dictionary<string, string> replacementMap)
        //{
        //}

        private MessageBoxResult CreateModal(string text, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            string messageBoxText = text;
            //MessageBoxButton messageBoxButton = button;
            //MessageBoxImage messageBoxIcon = icon;
            MessageBoxResult messageBoxResult;

            messageBoxResult = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
            //Help.ShowPopup(saveTableBtn, $"As of now, composite combinations are not supported: {compositeCombos[1] + compositeCombos[0]} = {compositeCombos[1]} + {compositeCombos[0]}. Please, delete either the composite combination or one of the components.", new Point(this.Left + saveTableBtn.Left, this.Top + saveTableBtn.Top));

            return messageBoxResult;
        }

        private bool VerifyCombos(Dictionary<string, string> replacementMap)
        {
            // first gotta figure out the combos. then gotta check whether some of them don't contain another ones
            string[] combos = liveTransliterator.ukrTranslit.getReplacementMapCombos(replacementMap);

            List<string> compositeCombos = checkForCompositeCombos(combos);

            if (compositeCombos.Count > 0)
            {
                // you gotta read more about the Help class. The gui looks sorta outdated.

                //Help.ShowPopup(saveTableBtn, $"Combination '{sub
                //0]}' is contained within '{subcombos[0]}'. As of now, contained combos are not suppported. Please, delete either of these.", new Point(this.Left + saveTableBtn.Left, this.Top + saveTableBtn.Top));
                string messageBoxText = $"As of now, composite combinations are not supported: {compositeCombos[1] + compositeCombos[0]} = {compositeCombos[1]} + {compositeCombos[0]}. Please, delete either the composite combination or one of the components.";
                string caption = "Warning";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBoxResult result;

                result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                //Help.ShowPopup(saveTableBtn, $"As of now, composite combinations are not supported: {compositeCombos[1] + compositeCombos[0]} = {compositeCombos[1]} + {compositeCombos[0]}. Please, delete either the composite combination or one of the components.", new Point(this.Left + saveTableBtn.Left, this.Top + saveTableBtn.Top));

                return false;
            }

            return true;
        }

        // composite combo is a combination of other combos
        public List<string> checkForCompositeCombos(string[] combos)
        {
            //combos.contain

            foreach (string combo in combos)
            {
                foreach (string combo2 in combos)
                {
                    if (combos.Contains(combo + combo2))
                    {
                        return new List<string> { combo2, combo };
                    }
                }
            }

            return new List<string>();
        }

        private void addRowBtn_Click(object sender, RoutedEventArgs e)
        {
            generatePanelWithEditBoxes("", "");
            //panel1.ScrollControlIntoView(panel1.Controls[panel1.Controls.Count - 1]);
            panel1.SetVerticalOffset(panel1.VerticalOffset);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //initializeTablesComboBox();
            var tablesComboBoxItemsBindingObject = new Binding("TranslitTables")
            {
                Mode = BindingMode.OneWay,
                Source = liveTransliterator.ukrTranslit
            };

            BindingOperations.SetBinding(comboBox1, ComboBox.ItemsSourceProperty, tablesComboBoxItemsBindingObject);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            void action() => this.InvalidateMeasure();
            this.Dispatcher.BeginInvoke((Action)action);
        }

        private void newTableFromJsonBtn_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "transliterationTable"; // Default file name
            dialog.DefaultExt = ".json"; // Default file extension
            dialog.Filter = "Text documents (.json)|*.json"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string pathToFile = dialog.FileName;
                string fileName = Path.GetFileName(pathToFile);

                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Resources\TranslitTables\{fileName}"), File.ReadAllText(pathToFile));
                liveTransliterator.ukrTranslit.TranslitTables.Add(fileName);
                //initializeTablesComboBox(true);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var WindowIsOffScreen = IsWindowOffScreen();

            if (WindowIsOffScreen.offsetDimesion != OffScreenDimension.None)
            {
                int padding = 50;
                if (WindowIsOffScreen.offsetDimesion == OffScreenDimension.Bottom)
                {
                    this.Top = this.Top - WindowIsOffScreen.val - padding;
                }
            }
        }

        public struct Offset
        {
            public OffScreenDimension offsetDimesion;
            public int val;

            public Offset(OffScreenDimension offsetDimesion, int val)
            {
                this.offsetDimesion = offsetDimesion;
                this.val = val;
            }
        }

        private void openTablesFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            string pathToTablesFolder = App.GetFullPath("Resources\\TranslitTables");

            string processArgsAsString = $"{pathToTablesFolder}";

            //Process.Start($"explorer.exe {pathToTablesFolder}");
            string[] processArgs = { "explorer.exe", pathToTablesFolder };
            var startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "explorer.exe";

            startInfo.Arguments = processArgsAsString;
            Process.Start(startInfo);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}