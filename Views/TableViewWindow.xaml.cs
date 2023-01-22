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
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using TextBox = System.Windows.Controls.TextBox;

namespace TransliteratorWPF_Version.Views
{
    public partial class TableViewWindow : Window
    {
        private readonly LiveTransliterator liveTransliterator;

        private App app = ((App)Application.Current);

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

        public TableViewWindow()
        {
            // TODO: Dependency injection 
            liveTransliterator = LiveTransliterator.GetInstance();

            InitializeComponent();
        }

        private void newTableBtn_Click(object sender, RoutedEventArgs e)
        {
            NewTableNameDialog dialog = new NewTableNameDialog();

            if (dialog.ShowDialog() == true)
            {
                string tableName = dialog.textBox1.Text;

                File.WriteAllText(Path.Combine(App.BaseDir, $@"Resources\TranslitTables\{tableName}.json"), "{}");

                liveTransliterator.ukrTranslit.TranslitTables.Add($"{tableName}.json");
                liveTransliterator.ukrTranslit.TranslitTables = liveTransliterator.ukrTranslit.TranslitTables.ToArray().ToList();
            }
            else
            {
            }
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
                debugWindow?.ConsoleLog($"A table needs to be selected.");
                return;
            }

            string tableName = comboBox1.SelectedItem.ToString();

            File.Delete(Path.Combine(App.BaseDir, $@"Resources\TranslitTables\{tableName}"));

            debugWindow?.ConsoleLog($"{tableName} has been deleted");

            liveTransliterator.ukrTranslit.TranslitTables.Remove(tableName);
            liveTransliterator.ukrTranslit.TranslitTables = liveTransliterator.ukrTranslit.TranslitTables.ToArray().ToList();
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                return;
            }

            string selectedTranslitTable = comboBox1.SelectedItem.ToString();
            panel1.Children.Clear();

            generateEditBoxesForTranslitTable(liveTransliterator.ukrTranslit.ReadReplacementMapFromJson(selectedTranslitTable));
        }

        public void generateEditBoxesForTranslitTable(Dictionary<string, string> replacement_map)
        {
            foreach (string key in liveTransliterator.ukrTranslit.SortReplacementMapKeys(replacement_map))
            {
                GeneratePanelWithEditBoxes(key, replacement_map[key]);
            }
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
            Screen currentMonitor = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow).Handle);

            PresentationSource source = PresentationSource.FromVisual(Application.Current.MainWindow);
            double dpiScaling = (source != null && source.CompositionTarget != null ? source.CompositionTarget.TransformFromDevice.M11 : 1);

            System.Drawing.Rectangle workArea = currentMonitor.WorkingArea;
            var workAreaWidth = (int)Math.Floor(workArea.Width * dpiScaling);
            var workAreaHeight = (int)Math.Floor(workArea.Height * dpiScaling);

            Application.Current.MainWindow.Left = (((workAreaWidth - (this.ActualWidth * dpiScaling)) / 2) + (workArea.Left * dpiScaling));
            Application.Current.MainWindow.Top = (((workAreaHeight - (this.ActualHeight * dpiScaling)) / 2) + (workArea.Top * dpiScaling));
        }

        public void GeneratePanelWithEditBoxes(string key, string value)
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
            int hostingPanelHeight = 20;

            StackPanel hostingPanel = new StackPanel();
            hostingPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            hostingPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            hostingPanel.Margin = new Thickness(0, verticalOffset, 0, 0);
            TextBox keyTxtBox = new TextBox();
            keyTxtBox.Text = key;
            keyTxtBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            keyTxtBox.TextAlignment = TextAlignment.Center;
            keyTxtBox.Margin = new Thickness(0, 0, 30, 0);
            keyTxtBox.IsEnabled = false;
            TextBox valueTxtBox = new TextBox();
            valueTxtBox.Text = value;
            valueTxtBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            valueTxtBox.TextAlignment = TextAlignment.Center;
            valueTxtBox.IsEnabled = false;

            hostingPanel.Children.Add(keyTxtBox);
            hostingPanel.Children.Add(valueTxtBox);

            panel1.Children.Add(hostingPanel);
        }

        private void saveTableBtn_Click(object sender, RoutedEventArgs e)
        {
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

            //File.WriteAllText(Path.Combine(App.BaseDir, $"Resources\\TranslitTables\\{comboBox1.SelectedItem}"), serializedTable);
            //string nameOfCurrentlyUsedReplacementMap = mainWindow.translitTablesComboBox.SelectedItem.ToString();
            //app.liveTranslit.ukrTranslit.SetReplacementMapFromJson(nameOfCurrentlyUsedReplacementMap);
        }

        private static MessageBoxResult CreateModal(string text, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            string messageBoxText = text;
            MessageBoxResult messageBoxResult;

            messageBoxResult = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
            return messageBoxResult;
        }

        private bool VerifyCombos(Dictionary<string, string> replacementMap)
        {
            string[] combos = liveTransliterator.ukrTranslit.getReplacementMapCombos(replacementMap);

            List<string> compositeCombos = CheckForCompositeCombos(combos);

            if (compositeCombos.Count > 0)
            {
                string messageBoxText = $"As of now, composite combinations are not supported: {compositeCombos[1] + compositeCombos[0]} = {compositeCombos[1]} + {compositeCombos[0]}. Please, delete either the composite combination or one of the components.";
                string caption = "Warning";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBoxResult result;

                result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                return false;
            }

            return true;
        }

        public List<string> CheckForCompositeCombos(string[] combos)
        {
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

        private void AddRowBtn_Click(object sender, RoutedEventArgs e)
        {
            GeneratePanelWithEditBoxes("", "");
            panel1.SetVerticalOffset(panel1.VerticalOffset);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var tablesComboBoxItemsBindingObject = new Binding("TranslitTables")
            {
                Mode = BindingMode.OneWay,
                Source = liveTransliterator.ukrTranslit
            };

            BindingOperations.SetBinding(comboBox1, ComboBox.ItemsSourceProperty, tablesComboBoxItemsBindingObject);

            int currentlySelectedTableIndex = liveTransliterator.ukrTranslit.selectedTranslitTableIndex;
            comboBox1.SelectedIndex = currentlySelectedTableIndex;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            void action() => this.InvalidateMeasure();
            this.Dispatcher.BeginInvoke((Action)action);
        }

        private void newTableFromJsonBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "transliterationTable";
            dialog.DefaultExt = ".json";
            dialog.Filter = "Text documents (.json)|*.json";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string pathToFile = dialog.FileName;
                string fileName = Path.GetFileName(pathToFile);

                File.WriteAllText(Path.Combine(App.BaseDir, $@"Resources\TranslitTables\{fileName}"), File.ReadAllText(pathToFile));
                liveTransliterator.ukrTranslit.TranslitTables.Add(fileName);
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