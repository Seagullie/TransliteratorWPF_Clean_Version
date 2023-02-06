using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TransliteratorWPF_Version.Views
{
    // TODO: Find a way for other windows to inherit from this class
    public class EnhancedWindow : Window
    {
        // make window draggable
        // TODO: make interface for draggable window?
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}