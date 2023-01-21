using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using TransliteratorWPF_Version.Properties;
using Application = System.Windows.Application;

namespace TransliteratorWPF_Version.Views
{
    public partial class SettingsWindow : Window
    {
        private App app = ((App)Application.Current);

        public DebugWindow? debugWindow
        {
            get
            {
                WindowCollection windows = System.Windows.Application.Current.Windows;
                foreach (Window window in windows)
                {
                    // warning: hardcoded
                    if (window.Name == "DebugWindow1")
                    {
                        return (DebugWindow)window;
                    }
                }
                return null;
            }
        }

        public StateOverlayWindow? stateOverlayWindow
        {
            get
            {
                WindowCollection windows = System.Windows.Application.Current.Windows;
                foreach (Window window in windows)
                {
                    // warning: hardcoded
                    if (window.Name == "StateOverlayWindow1")
                    {
                        return (StateOverlayWindow)window;
                    }
                }

                return null;
            }
        }

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void playSoundOnTranslitToggleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.playSoundOnTranslitToggle = true;
        }

        private void playSoundOnTranslitToggleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.playSoundOnTranslitToggle = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            playSoundOnTranslitToggleCheckBox.IsChecked = Settings.Default.playSoundOnTranslitToggle;

            shortcutInputBox.Text = Settings.Default.toggleTranslitShortcut;
            startMinimizedCheckBox.IsChecked = Settings.Default.startMinimized;
            turnOnTranslitAtStartCheckBox.IsChecked = Settings.Default.turnOnTranslitAtStart;
            enableStateOverlayWindowCheckBox.IsChecked = Settings.Default.enableStateOverlayWindow;

            launchProgramOnSystemStartupCheckBox.IsChecked = Settings.Default.launchProgramOnSystemStartup;
            suppressAltShiftCheckBox.IsChecked = Settings.Default.suppressAltShift;

            launchProgramOnSystemStartupCheckBox.IsEnabled = App.IsAdministrator();

            displayRadioBtn.IsChecked = Settings.Default.displayCombos;
            waitForComboRadioBtn.IsChecked = !Settings.Default.displayCombos;

            displayRadioBtn.IsEnabled = true;
            waitForComboRadioBtn.IsEnabled = true;
        }

        private void shortcutInputBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            shortcutInputBox.Clear();

            debugWindow?.ConsoleLog($"e.Key: {e.Key} and e.Systemkey {e.SystemKey}");

            Key wpfKey = e.Key;

            if (wpfKey == Key.System)
            {
                wpfKey = e.SystemKey;
            }

            Keys formsKey;
            formsKey = (Keys)KeyInterop.VirtualKeyFromKey(wpfKey);

            bool altDown = app.liveTranslit.keyLogger.keyStateChecker.IsKeyDown(Key.LeftAlt);
            bool ctrlDown = app.liveTranslit.keyLogger.keyStateChecker.IsKeyDown(Key.LeftCtrl);
            string[] modifiersDown = { altDown ? "Alt" : "", ctrlDown ? "Control" : "" };
            modifiersDown = modifiersDown.Where(modifier => modifier != "").ToArray<string>();

            string settingsString = $"{String.Join(" + ", modifiersDown)} + {formsKey}";

            Settings.Default.toggleTranslitShortcut = settingsString;
            app.liveTranslit.keyLogger.fetchToggleTranslitShortcutFromSettings();

            shortcutInputBox.Text = settingsString;

            e.Handled = true;
        }

        public async void ShowcaseComboMode()
        {
            MainWindow mainWindow = (MainWindow)app.MainWindow;

            // switch translit table to accentsTable
            // warning: hardcoded
            const string accentsTable = "tableAccents.json";
            string previousTable = app.liveTranslit.ukrTranslit.replacement_map_filename;

            app.liveTranslit.ukrTranslit.SetReplacementMapFromJson($"{accentsTable}");

            // gotta make sure transliterator is enabled. Should reference .keyLogger.state, not liveTranslit.state
            if (app.liveTranslit.keyLogger.State == false)
            {
                mainWindow.toggleTranslit_Click();
            }

            DOCshowcaseTxtBox.IsEnabled = true;
            DOCshowcaseTxtBox.Focus();
            DOCshowcaseTxtBox.Clear();

            // warning: hardcoded
            string testString = "`a`o`i`u";

            await app.liveTranslit.WriteInjected(testString);
            DOCshowcaseTxtBox.IsEnabled = false;

            app.liveTranslit.ukrTranslit.SetReplacementMapFromJson($"{previousTable}");
        }

        private void displayRadioBtn_Checked_1(object sender, RoutedEventArgs e)
        {
            if (displayRadioBtn.IsEnabled != true)
            {
                return;
            }

            Settings.Default.displayCombos = true;
            app.liveTranslit.displayCombos = true;

            if (debugWindow != null && debugWindow.underTestByWinDriverCheckBox.IsChecked == true)
            {
                return;
            }
            else
            {
                ShowcaseComboMode();
            }
        }

        private void waitForComboRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (waitForComboRadioBtn.IsEnabled != true)
            {
                return;
            }

            Settings.Default.displayCombos = false;
            app.liveTranslit.displayCombos = false;

            if (debugWindow != null && debugWindow.underTestByWinDriverCheckBox.IsChecked == true)
            {
                return;
            }
            else
            {
                ShowcaseComboMode();
            }
        }

        private void editTranslitTablesBtn_Click(object sender, RoutedEventArgs e)
        {
            TranslitTablesWindow translitTables = new TranslitTablesWindow();
            translitTables.Show();
        }

        public void FixSizeToContent()
        {
            var window = this;

            window.Activated += (s, e) => OnActivated();

            void OnActivated()
            {
                window.Activated -= (s, e) => OnActivated();

                void action() => window.InvalidateMeasure();
                window.Dispatcher.BeginInvoke((Action)action);
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            void action() => this.InvalidateMeasure();
            this.Dispatcher.BeginInvoke((Action)action);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.startMinimized = startMinimizedCheckBox.IsChecked == true;
            Settings.Default.turnOnTranslitAtStart = turnOnTranslitAtStartCheckBox.IsChecked == true;

            Settings.Default.launchProgramOnSystemStartup = launchProgramOnSystemStartupCheckBox.IsChecked == true;

            Settings.Default.enableStateOverlayWindow = enableStateOverlayWindowCheckBox.IsChecked == true;
            Settings.Default.suppressAltShift = suppressAltShiftCheckBox.IsChecked == true;

            Settings.Default.Save();
        }

        private void launchProgramOnSystemStartupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!App.IsAdministrator())
            {
                return;
            }

            var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(path, true);

            string pathToExecutable = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
            key.SetValue(app.appName, pathToExecutable);
        }

        private void launchProgramOnSystemStartupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!App.IsAdministrator())
            {
                return;
            }

            var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
            key.DeleteValue(app.appName, false);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void editToggleSoundsBtn_Click(object sender, RoutedEventArgs e)
        {
            var editToggleSoundsWindow = new EditToggleSoundsWindow();
            editToggleSoundsWindow.ShowDialog();
        }

        private void enableStateOverlayWindowCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!stateOverlayWindow.IsVisible)
            {
                stateOverlayWindow.Show();
            }
        }

        private void enableStateOverlayWindowCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (stateOverlayWindow.IsVisible)
            {
                stateOverlayWindow.Hide();
            }
        }

        private void suppressAltShiftCheckBox_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void suppressAltShiftCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
        }
    }
}