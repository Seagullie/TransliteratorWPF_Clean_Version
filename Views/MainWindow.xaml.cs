using ModernWpf;
using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TransliteratorWPF_Version.Helpers;
using TransliteratorWPF_Version.Properties;
using TransliteratorWPF_Version.Services;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using ComboBox = System.Windows.Controls.ComboBox;
using Label = System.Windows.Controls.Label;

namespace TransliteratorWPF_Version.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SoundPlayer soundCont = new SoundPlayer(TransliteratorWPF_Version.Properties.Resources.cont);

        private SoundPlayer soundPause = new SoundPlayer(TransliteratorWPF_Version.Properties.Resources.pause);
        public System.Windows.Forms.NotifyIcon ni;

        public App app
        {
            get
            {
                return ((App)Application.Current);
            }
        }

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

        public MainWindow()
        {
            InitializeComponent();

            ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = TransliteratorWPF_Version.Properties.Resources.keyboardIconMarginless;
            ni.Visible = true;
            ni.Click +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        private void launchDebugWindowBtn_Click(object sender, RoutedEventArgs e)
        {
            DebugWindow debugWindow = new DebugWindow();
            debugWindow.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.startMinimized)
            {
                this.WindowState = WindowState.Minimized;
            }

            if (Settings.Default.selectedTheme != "")
            {
                if (Settings.Default.selectedTheme == "Dark")
                {
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                }
                else
                {
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                }
            }

            app.liveTranslit = LiveTransliterator.GetInstance();

            // fetching last selected table from preferences and notifying backend about that:
            app.liveTranslit.ukrTranslit.selectedTranslitTableIndex = app.liveTranslit.ukrTranslit.translitTables.FindIndex((tableName) => tableName == Settings.Default.lastTranslitTable);

            // -- tablesComboBoxItems binding --

            // warning: hardcoded
            var tablesComboBoxItemsBindingObject = new Binding("translitTables")
            {
                Mode = BindingMode.OneWay,
                Source = app.liveTranslit.ukrTranslit
            };

            BindingOperations.SetBinding(translitTablesComboBox, ComboBox.ItemsSourceProperty, tablesComboBoxItemsBindingObject);

            // -- tablesComboBoxSelectedIndex desc binding--

            // warning: hardcoded
            var tablesComboBoxSelectedIndex = new Binding("selectedTranslitTableIndex")
            {
                Mode = BindingMode.TwoWay,
                Source = app.liveTranslit.ukrTranslit
            };

            BindingOperations.SetBinding(translitTablesComboBox, ComboBox.SelectedIndexProperty, tablesComboBoxSelectedIndex);

            // -- state desc binding--
            // warning: hardcoded
            var stateBindingObject = new Binding("stateDesc")
            {
                // Configure the binding
                Mode = BindingMode.OneWay,
                Source = app.liveTranslit.keyLogger
            };
            BindingOperations.SetBinding(stateLabel, Label.ContentProperty, stateBindingObject);

            // -- state desc color binding--

            //var stateColorBindingObject = new Binding("stateDesc")
            //{
            //    // Configure the binding
            //    Mode = BindingMode.OneWay,
            //    Source = app.liveTranslit.keyLogger
            //};

            //IValueConverter converterFunc = new StateToColorConverter();
            //stateColorBindingObject.Converter = converterFunc;

            //BindingOperations.SetBinding(stateLabel, Label.ForegroundProperty, stateColorBindingObject);

            // -- state enabled color binding--

            // warning: hardcoded
            var stateEnabledBindingObject = new Binding("stateDesc")
            {
                // Configure the binding
                Mode = BindingMode.OneWay,
                Source = app.liveTranslit.keyLogger
            };

            //stateEnabledBindingObject.Converter =
            stateEnabledBindingObject.Converter = new IntToBoolConverter();

            BindingOperations.SetBinding(stateLabel, Label.IsEnabledProperty, stateEnabledBindingObject);

            // -- shortcut desc binding--

            // warning: hardcoded
            var shortcutDescBindingObject = new Binding("ToggleTranslitShortcut")
            {
                // Configure the binding
                Mode = BindingMode.OneWay,
                Source = app.liveTranslit.keyLogger,
                Converter = new ShortcutHashSetToStringConverter()
            };

            BindingOperations.SetBinding(translitToggleShortcutLabel, Label.ContentProperty, shortcutDescBindingObject);

            StateOverlayWindow stateOveralyWindow = new StateOverlayWindow();

            if (Settings.Default.enableStateOverlayWindow)
            {
                stateOveralyWindow.Show();
            }
        }

        public void initializeTablesComboBox(bool reinitialze = false)
        {
            int selectedIndexBeforeReinitialization = translitTablesComboBox.SelectedIndex;

            if (reinitialze)
            {
                translitTablesComboBox.Items.Clear();
            }

            string[] tables = app.liveTranslit.ukrTranslit.translitTables.ToArray();

            foreach (string table in tables)
            {
                translitTablesComboBox.Items.Add(table);
            }

            translitTablesComboBox.SelectedIndex = app.liveTranslit.ukrTranslit.selectedTranslitTableIndex;
        }

        private void translitTablesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (translitTablesComboBox.SelectedItem == null)
            {
                return;
            }

            string selectedTable = translitTablesComboBox.SelectedItem.ToString();
            app.liveTranslit.ukrTranslit.SetReplacementMapFromJson($"{selectedTable}");
        }

        public void toggleTranslit_Click()
        {
            app.liveTranslit.keyLogger.Toggle();
            string stateDesc = (app.liveTranslit.keyLogger.State == true ? "On" : "Off");

            if (Settings.Default.playSoundOnTranslitToggle)
            {
                Sound sound = new Sound();

                string pathToSoundToPlay;

                if (app.liveTranslit.keyLogger.State == true)
                {
                    if (Settings.Default.pathToCustomToggleOnSound != "")
                    {
                        pathToSoundToPlay = Properties.Settings.Default.pathToCustomToggleOnSound;
                    }
                    else
                    {
                        // warning: hardcoded
                        pathToSoundToPlay = System.IO.Path.Combine(App.BaseDir, $"Resources/{(app.liveTranslit.keyLogger.State == true ? "cont" : "pause")}.wav");
                    }
                }
                else
                {
                    if (Properties.Settings.Default.pathToCustomToggleOffSound != "")
                    {
                        pathToSoundToPlay = Properties.Settings.Default.pathToCustomToggleOffSound;
                    }
                    else
                    {
                        // warning: hardcoded
                        pathToSoundToPlay = System.IO.Path.Combine(App.BaseDir, $"Resources/{(app.liveTranslit.keyLogger.State == true ? "cont" : "pause")}.wav");
                    }
                }

                //pathToSoundToPlay = System.IO.Path.GetFullPath(System.IO.Path.Combine(App.BaseDir, $"Resources/{(app.liveTranslit.keyLogger.state == 1 ? "cont" : "pause")}.wav"));
                sound.Play(pathToSoundToPlay);
            }
        }

        public Color ChangeLightness(Color color, float percent)
        {
            return Color.FromArgb(color.A, (byte)(color.R * percent), (byte)(color.G * percent), (byte)(color.B * percent));
        }

        //public Color BakeTransparency(Color color, float transparency)
        //{
        //    return Color.FromArgb(color.A), (byte)(color.R), (byte)(color.G), (byte)(color.B));
        //}

        private void toggleBtb_Click(object sender, RoutedEventArgs e)
        {
            toggleTranslit_Click();
        }

        private void settingsBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Show();
        }

        private void changeThemeBtn_Click(object sender, RoutedEventArgs e)
        {
            // .ApplicationTheme isn't set on the start

            if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                Properties.Settings.Default.selectedTheme = "Light";
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                Properties.Settings.Default.selectedTheme = "Dark";
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            void action() => this.InvalidateMeasure();
            this.Dispatcher.BeginInvoke((Action)action);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ni.Dispose();
            Properties.Settings.Default.lastTranslitTable = app.liveTranslit.ukrTranslit.translitTables[app.liveTranslit.ukrTranslit.selectedTranslitTableIndex];
            Properties.Settings.Default.Save();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            // also gotta generate notification that the app is in the tray
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            // for some reason, I'm getting stack overflow here.
            // but calling base is supposed to prevent that

            //base.OnStateChanged(e);
        }

        // this doesn't trigger
        private void stateLabel_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            var targetObject = (Label)(e.TargetObject);

            if (targetObject.Content.ToString() == "On")
            {
                targetObject.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                targetObject.Foreground = new SolidColorBrush(Colors.Green);
            }
        }

        // this also doesn't trigger
        private void stateLabel_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            var targetObject = (Label)(e.TargetObject);

            if (targetObject.Content.ToString() == "On")
            {
                targetObject.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                targetObject.Foreground = new SolidColorBrush(Colors.Green);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
        }

        private void showTableBtn_Click(object sender, RoutedEventArgs e)
        {
            TableViewWindow tableViewWindow = new TableViewWindow();
            tableViewWindow.Show();
        }
    }
}