using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace TransliteratorWPF_Version.Views
{
    /// <summary>
    /// Interaction logic for StateOverlayWindow.xaml
    /// </summary>

    public partial class StateOverlayWindow : Window
    {
        public App app
        {
            get
            {
                return ((App)Application.Current);
            }
        }

        public StateOverlayWindow()
        {
            InitializeComponent();
        }

        private void StateOverlayWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            this.Top = 50;

            // should be screen width - window width - desired margin

            var allScreens = System.Windows.Forms.Screen.AllScreens.ToList();
            var thisScreen = allScreens.SingleOrDefault(s => this.Left >= s.WorkingArea.Left && this.Left < s.WorkingArea.Right);

            int screenHeight = (int)SystemParameters.FullPrimaryScreenHeight;
            int screenWidth = (int)SystemParameters.FullPrimaryScreenWidth;

            int windowWidth = (int)this.ActualWidth;
            int desiredMargin = 50;

            // this doesn't work. The overlay goes offscreen
            // this is probably related to screen scaling. How do I handle that?

            this.Left = (int)(screenWidth - windowWidth - desiredMargin);
            //string pathToImage = @"C:\Users\main\source\repos\TransliteratorWPF_Version\bin\Debug\netcoreapp3.1\Resources\keyboardIcon.png";
            string pathToImage = @"Resources\keyboardIconMarginless.png";

            // this doesn't work. You better convert it to absolute path

            Uri uriToImage = new Uri(Path.Combine(App.BaseDir, pathToImage), UriKind.Absolute);
            iconImg.Source = new BitmapImage(uriToImage);

            // -- state enabled color binding--

            var stateEnabledBindingObject = new Binding("stateDesc")
            {
                // Configure the binding
                Mode = BindingMode.OneWay,
                Source = app.liveTranslit.keyLogger
            };

            //stateEnabledBindingObject.Converter =
            stateEnabledBindingObject.Converter = new StateToColorConverter();

            BindingOperations.SetBinding(stateRect, Rectangle.FillProperty, stateEnabledBindingObject);
        }

        private void StateOverlayWindow1_ContentRendered(object sender, EventArgs e)
        {
        }

        private class StateToColorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                string val = (string)value;

                if (val == "On")
                {
                    return "Green";
                }
                else
                {
                    return "Red";
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return false;
            }
        }
    }
}