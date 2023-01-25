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

        private void Window_Activated(object sender, EventArgs e)
        {
            // TODO: Delete this?
            void action() => InvalidateMeasure();
            Dispatcher.BeginInvoke((Action)action);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.SaveAllProps();
        }
    }
}