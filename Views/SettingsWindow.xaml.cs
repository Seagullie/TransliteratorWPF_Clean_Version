using System;
using System.Windows;
using System.Windows.Input;
using TransliteratorWPF_Version.ViewModels;

namespace TransliteratorWPF_Version.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsViewModel ViewModel { get; private set; }

        public SettingsWindow()
        {
            InitializeComponent();
            ViewModel = new();
            DataContext = ViewModel;
        }

        // TODO: Remove this duct tape
        private void Window_Activated(object sender, EventArgs e)
        {
            // Remove unpainted white area around borders
            void action() => InvalidateMeasure();
            Dispatcher.BeginInvoke((Action)action);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Make window draggable
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.InitializePropertiesFromSettings();
        }
    }
}