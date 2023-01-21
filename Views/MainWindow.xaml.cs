using System;
using System.Windows;
using System.Windows.Input;
using TransliteratorWPF_Version.ViewModels;

namespace TransliteratorWPF_Version.Views
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel {  get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new();
            DataContext = ViewModel;        
        }

        // TODO: Try udnerstand this logic
        private void Window_Activated(object sender, EventArgs e)
        {
            void action() => InvalidateMeasure();
            Dispatcher.BeginInvoke(action);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.SaveSettingsAndDispose();
        }
    }
}