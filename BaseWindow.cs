using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace TranslitBaseWindow
{
    public class BaseWindow : Window
    {
        // remove unpainted white area around borders
        private void Window_Activated(object sender, EventArgs e)
        {
            void action() => this.InvalidateMeasure();
            this.Dispatcher.BeginInvoke((Action)action);
        }

        // make window draggable
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}