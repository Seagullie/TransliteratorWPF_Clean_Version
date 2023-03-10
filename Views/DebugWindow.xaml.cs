using System;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TransliteratorWPF_Version.Services;
using TransliteratorWPF_Version.ViewModels;
using MouseButton = System.Windows.Input.MouseButton;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace TransliteratorWPF_Version.Views
{
    public partial class DebugWindow : Window
    {
        private DebugViewModel ViewModel;
        private LoggerService loggerService;
        private Main liveTransliterator;

        public DebugWindow()
        {
            loggerService = LoggerService.GetInstance();
            liveTransliterator = Main.GetInstance();

            loggerService.NewLogMessage += ConsoleLog;

            ViewModel = new();
            DataContext = ViewModel;

            InitializeComponent();
        }

        public void ConsoleLog(object sender, NewLogMessageEventArg e)
        {
            if (!ViewModel.logsEnabled)
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

        // TODO: Transfer the bindings to XAML
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // --

            // TODO: Fix XAML binding equivalent

            //var stateColorBindingObject = new Binding("StateDesc")
            //{
            //    Mode = BindingMode.OneWay,
            //    Source = liveTransliterator.keyLogger
            //};

            //IValueConverter converterFunc = new StateToColorConverter();
            //stateColorBindingObject.Converter = converterFunc;

            //BindingOperations.SetBinding(StateLabel, Label.ForegroundProperty, stateColorBindingObject);

            // --

            loggerService.LogMessage(this, "Up And Running");
            loggerService.LogMessage(this, $"BaseDir is: {AppDomain.CurrentDomain.BaseDirectory}");
        }

        // can't move to view model cause it references a control
        private async void simulateKeyboardInputBtn_Click(object sender, RoutedEventArgs e)
        {
            textBox1.Focus();
            await liveTransliterator.keyInjectorService.WriteInjected("simulated");
        }

        // Make window draggable
        private void MainWindow1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // TODO: Remove the temporary fix
        private void MainWindow1_Activated(object sender, EventArgs e)
        {
            void action() => this.InvalidateMeasure();
            this.Dispatcher.BeginInvoke((Action)action);
        }

        private void DebugWindow1_Closed(object sender, EventArgs e)
        {
            // unregister log event handler to avoid duplication once the window is opened again & disable logging when DebugWindow is not open
            loggerService.NewLogMessage -= ConsoleLog;
        }
    }
}