using System;
using System.Linq;
using System.Windows;
using TransliteratorWPF_Version.ViewModels;

namespace TransliteratorWPF_Version.Views
{
    /// <summary>
    /// Interaction logic for StateOverlayWindow.xaml
    /// </summary>

    public partial class StateOverlayWindow : Window
    {
        public StateOverlayViewModel ViewModel { get; private set; }

        public StateOverlayWindow()
        {
            InitializeComponent();
            ViewModel = new();
            DataContext = ViewModel;
        }

        private void StateOverlayWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            Top = 50;

            var allScreens = System.Windows.Forms.Screen.AllScreens.ToList();
            var thisScreen = allScreens.SingleOrDefault(s => this.Left >= s.WorkingArea.Left && this.Left < s.WorkingArea.Right);

            int screenHeight = (int)SystemParameters.FullPrimaryScreenHeight;
            int screenWidth = (int)SystemParameters.FullPrimaryScreenWidth;

            int windowWidth = (int)this.ActualWidth;
            int desiredMargin = 50;

            // should be screen width - window width - desired margin

            // this doesn't work. The overlay goes offscreen
            // this is probably related to screen scaling. How do I handle that?

            Left = screenWidth - windowWidth - desiredMargin;
        }

        private void StateOverlayWindow1_ContentRendered(object sender, EventArgs e)
        {
        }
    }
}