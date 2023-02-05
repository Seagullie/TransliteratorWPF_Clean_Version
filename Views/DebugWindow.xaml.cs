﻿using ModernWpf;
using System;
using System.IO;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TransliteratorWPF_Version.Helpers;
using TransliteratorWPF_Version.Services;
using WindowsInput;
using WindowsInput.Native;
using Binding = System.Windows.Data.Binding;
using ComboBox = System.Windows.Controls.ComboBox;
using Label = System.Windows.Controls.Label;
using MouseButton = System.Windows.Input.MouseButton;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace TransliteratorWPF_Version.Views
{
    public partial class DebugWindow : Window
    {
        private SoundPlayer soundCont;

        private SoundPlayer soundPause;

        public bool logsEnabled = true;

        private readonly Main liveTransliterator;
        private readonly LoggerService loggerService;
        private readonly SettingsService settingsService;

        public DebugWindow()
        {
            // TODO: Dependency injection
            liveTransliterator = Main.GetInstance();
            settingsService = SettingsService.GetInstance();
            loggerService = LoggerService.GetInstance();

            loggerService.NewLogMessage += ConsoleLog;

            string str = "TransliteratorWPF_Version.Resources.Audio.cont.wav";
            Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(str);
            soundCont = new(s);

            str = "TransliteratorWPF_Version.Resources.Audio.pause.wav";
            s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(str);
            soundPause = new(s);
            InitializeComponent();
        }

        public static void Send(Key key)
        {
            if (Keyboard.PrimaryDevice != null)
            {
                if (Keyboard.PrimaryDevice.ActiveSource != null)
                {
                    var e = new System.Windows.Input.KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key)
                    {
                        RoutedEvent = Keyboard.KeyDownEvent
                    };
                    InputManager.Current.ProcessInput(e);
                }
            }
        }

        public static void SimulateKeyPressThroughInputSimulator(VirtualKeyCode keyCode)
        {
            var inputSimulator = new InputSimulator();
            inputSimulator.Keyboard.KeyDown(keyCode);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(3000);
            WinAPI.WriteStringThroughSendInput("test");
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
        }

        public void initializeTablesComboBox(bool reinitialze = false)
        {
            int selectedIndexBeforeReinitialization = translitTablesBox.SelectedIndex;

            if (reinitialze)
            {
                translitTablesBox.Items.Clear();
            }

            string[] tables = getAllTranslitTableNames();

            foreach (string table in tables)
            {
                translitTablesBox.Items.Add(table);
            }

            translitTablesBox.SelectedIndex = selectedIndexBeforeReinitialization >= (translitTablesBox.Items.Count - 1) ? translitTablesBox.Items.Count - 1 : selectedIndexBeforeReinitialization;
        }

        public string[] getAllTranslitTableNames()
        {
            string[] tables = Utilities.getAllFilesInFolder(@"Resources/TranslitTables");

            return tables;
        }

        public void ConsoleLog(object sender, NewLogMessageEventArg e)
        {
            if (!logsEnabled)
            {
                return;
            }

            var message = e.Message;
            message += "\n";

            var color = e.Color;

            if (color != null)
            {
                AppendColoredText(outputTextBox, message, color.ToString());
            }
            else
            {
                outputTextBox.AppendText(message);
            }

            outputTextBox.ScrollToEnd();
        }

        public void AppendColoredText(RichTextBox box, string text, string color)
        {
            BrushConverter bc = new BrushConverter();
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                    bc.ConvertFromString(color));
            }
            catch (FormatException) { }
        }

        public void toggleTranslit_Click()
        {
            liveTransliterator.keyLogger.State = !liveTransliterator.keyLogger.State;
            string stateDesc = (liveTransliterator.keyLogger.State == true ? "On" : "Off");

            if ((settingsService.IsToggleSoundOn))
            {
                SoundPlayer soundToPlay = liveTransliterator.keyLogger.State == true ? soundCont : soundPause;
                soundToPlay.Play();
            }

            loggerService.LogMessage(this, $"Translit {stateDesc}");
            StateLabel.Content = stateDesc;
            if (liveTransliterator.keyLogger.State == true)
            {
                StateLabel.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                StateLabel.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void toggleTranslitBtn_Click(object sender, RoutedEventArgs e)
        {
            toggleTranslit_Click();
        }

        private void underTestByWinDriverCheckBox_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void translitTablesBox_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void translitTablesBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (translitTablesBox.SelectedItem == null)
            {
                return;
            }

            string selectedTable = translitTablesBox.SelectedItem.ToString();
            liveTransliterator.transliteratorService.SetTableModel(selectedTable);
        }

        private void testTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void settingsBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Show();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var stateBindingObject = new Binding("StateDesc")
            {
                Mode = BindingMode.OneWay,
                Source = liveTransliterator.keyLogger
            };

            BindingOperations.SetBinding(StateLabel, Label.ContentProperty, stateBindingObject);

            var stateColorBindingObject = new Binding("StateDesc")
            {
                Mode = BindingMode.OneWay,
                Source = liveTransliterator.keyLogger
            };

            IValueConverter converterFunc = new StateToColorConverter();
            stateColorBindingObject.Converter = converterFunc;

            BindingOperations.SetBinding(StateLabel, Label.ForegroundProperty, stateColorBindingObject);

            var tablesComboBoxItemsBindingObject = new Binding("TranslitTables")
            {
                Mode = BindingMode.OneWay,
                Source = liveTransliterator.transliteratorService
            };

            BindingOperations.SetBinding(translitTablesBox, ComboBox.ItemsSourceProperty, tablesComboBoxItemsBindingObject);

            var tablesComboBoxSelectedIndex = new Binding("selectedTranslitTableIndex")
            {
                Mode = BindingMode.TwoWay,
                Source = liveTransliterator.transliteratorService
            };

            BindingOperations.SetBinding(translitTablesBox, ComboBox.SelectedIndexProperty, tablesComboBoxSelectedIndex);

            loggerService.LogMessage(this, "Up And Running");
            loggerService.LogMessage(this, $"BaseDir is: {AppDomain.CurrentDomain.BaseDirectory}");
        }

        private void outputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void allowInjectedKeysButton_Click(object sender, RoutedEventArgs e)
        {
            liveTransliterator.keyLogger.gkh.alwaysAllowInjected = true;
        }

        private void toggleLogsBtn_Click(object sender, RoutedEventArgs e)
        {
            logsEnabled = !logsEnabled;
        }

        private void showTranslitTableBtn_Click(object sender, RoutedEventArgs e)
        {
            string serializedTable = Newtonsoft.Json.JsonConvert.SerializeObject(liveTransliterator.transliteratorService.transliterationTableModel.replacementTable, Newtonsoft.Json.Formatting.Indented);
            loggerService.LogMessage(this, "\n" + serializedTable);
        }

        private void checkCaseButtonsBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isCAPSLOCKon = liveTransliterator.keyLogger.keyStateChecker.IsCAPSLOCKon();
            bool isShiftPressedDown = liveTransliterator.keyLogger.keyStateChecker.IsShiftPressedDown();

            loggerService.LogMessage(this, $"CAPS LOCK on: {isCAPSLOCKon}. SHIFT pressed down: {isShiftPressedDown}");
        }

        private async void simulateKeyboardInputBtn_Click(object sender, RoutedEventArgs e)
        {
            textBox1.Focus();
            await liveTransliterator.keyInjectorService.WriteInjected("simulated");
        }

        private void getKeyLoggerMemoryBtn_Click(object sender, RoutedEventArgs e)
        {
            string memory = liveTransliterator.keyLogger.GetMemoryAsString();
            loggerService.LogMessage(this, $"Key Logger memory: [{memory}]");
        }

        private void allowInjectedKeysBtn_Click(object sender, RoutedEventArgs e)
        {
            liveTransliterator.keyLogger.gkh.alwaysAllowInjected = true;
        }

        private void slowDownKBEInjectionsBtn_Click(object sender, RoutedEventArgs e)
        {
            liveTransliterator.slowDownKBEInjections = 2000;
        }

        private void getLayoutBtn_Click(object sender, RoutedEventArgs e)
        {
            string layout = Utilities.GetCurrentKbLayout();
            loggerService.LogMessage(this, layout);
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void minimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MainWindow1_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void MainWindow1_StateChanged(object sender, EventArgs e)
        {
        }

        private void MainWindow1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void changeThemeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }
        }

        private void MainWindow1_Activated(object sender, EventArgs e)
        {
            void action() => this.InvalidateMeasure();
            this.Dispatcher.BeginInvoke((Action)action);
        }

        private void launchMainWindowBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void DebugWindow1_Closed(object sender, EventArgs e)
        {
            // unregister log event handler to avoid duplication once the window is opened again & disable logging when DebugWindow is not open
            loggerService.NewLogMessage -= ConsoleLog;
        }

        private void DebugWindow1_Closed_1(object sender, EventArgs e)
        {
        }
    }
}