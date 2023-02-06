using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using TextBox = System.Windows.Controls.TextBox;
using TransliteratorWPF_Version.Models;
using TransliteratorWPF_Version.ViewModels;

namespace TransliteratorWPF_Version.Views
{
    // I simply copypasted this code-behind from TranslitTablesWindow
    // I'm sure there is a better way to reuse UI
    // TODO: Refactor
    public partial class TableViewWindow : Window
    {
        private TableViewModel ViewModel;

        public TableViewWindow()
        {
            InitializeComponent();
        }

        private void tableComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tableComboBox.SelectedItem == null)
            {
                return;
            }

            // TODO: Refactor
            TransliterationTableModel tableModel = new TransliterationTableModel(tableComboBox.SelectedItem.ToString());

            // sometimes it is and I'm not sure why
            if (panel1 != null)
            {
                panel1?.Children.Clear();
                GenerateEditBoxesForTranslitTable(tableModel);
            }
        }

        public void GenerateEditBoxesForTranslitTable(TransliterationTableModel tableModel)
        {
            foreach (string key in tableModel.keys)
            {
                GeneratePanelWithEditBoxes(key, tableModel.replacementTable[key]);
            }
        }

        public enum OffScreenDimension
        {
            Top, Left, Bottom, Right, None
        }

        // TODO: Rename
        public Offset IsWindowOffScreen()
        {
            var allScreens = Screen.AllScreens.ToList();
            var thisScreen = allScreens.SingleOrDefault(s => this.Left >= s.WorkingArea.Left && this.Left < s.WorkingArea.Right);

            int top = (int)this.Top;
            int left = (int)this.Left;

            if (top < 0)
            {
                return new Offset(OffScreenDimension.Top, top * -1);
            }
            if (left < 0)
            {
                return new Offset(OffScreenDimension.Left, left * -1);
            }

            int bottom = (int)(top + this.ActualHeight);
            int right = (int)(left + this.ActualWidth);

            int taskBarHeightInPixels = 40;

            if (bottom > (thisScreen.Bounds.Bottom - taskBarHeightInPixels))
            {
                return new Offset(OffScreenDimension.Bottom, bottom - (thisScreen.Bounds.Bottom - taskBarHeightInPixels));
            }

            if (right > thisScreen.Bounds.Right)
            {
                return new Offset(OffScreenDimension.Right, right - thisScreen.Bounds.Right);
            }

            return new Offset(OffScreenDimension.None, 0);
        }

        // 0 references
        // TODO: Connect
        public void CenterWindow()
        {
            Screen currentMonitor = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow).Handle);

            PresentationSource source = PresentationSource.FromVisual(Application.Current.MainWindow);
            double dpiScaling = (source != null && source.CompositionTarget != null ? source.CompositionTarget.TransformFromDevice.M11 : 1);

            System.Drawing.Rectangle workArea = currentMonitor.WorkingArea;
            var workAreaWidth = (int)Math.Floor(workArea.Width * dpiScaling);
            var workAreaHeight = (int)Math.Floor(workArea.Height * dpiScaling);

            Application.Current.MainWindow.Left = (((workAreaWidth - (this.ActualWidth * dpiScaling)) / 2) + (workArea.Left * dpiScaling));
            Application.Current.MainWindow.Top = (((workAreaHeight - (this.ActualHeight * dpiScaling)) / 2) + (workArea.Top * dpiScaling));
        }

        public void GeneratePanelWithEditBoxes(string key, string value)
        {
            int verticalOffset;

            if (panel1.Children.Count == 0)
            {
                verticalOffset = 25;
            }
            else
            {
                StackPanel lastChild = (StackPanel)panel1.Children[^1];
                verticalOffset = (int)(lastChild.Margin.Top);
            }

            StackPanel hostingPanel = new StackPanel();
            hostingPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            hostingPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            hostingPanel.Margin = new Thickness(0, verticalOffset, 0, 0);
            TextBox keyTxtBox = new TextBox();
            keyTxtBox.Text = key;
            keyTxtBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            keyTxtBox.TextAlignment = TextAlignment.Center;
            keyTxtBox.Margin = new Thickness(0, 0, 30, 0);
            keyTxtBox.IsEnabled = false;

            TextBox valueTxtBox = new TextBox();
            valueTxtBox.Text = value;
            valueTxtBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            valueTxtBox.TextAlignment = TextAlignment.Center;
            valueTxtBox.IsEnabled = false;

            hostingPanel.Children.Add(keyTxtBox);
            hostingPanel.Children.Add(valueTxtBox);

            panel1.Children.Add(hostingPanel);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = new TableViewModel();
            DataContext = ViewModel;
        }

        // TODO: Remove the temporary fix
        private void Window_Activated(object sender, EventArgs e)
        {
            void action() => this.InvalidateMeasure();
            this.Dispatcher.BeginInvoke((Action)action);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var WindowIsOffScreen = IsWindowOffScreen();

            if (WindowIsOffScreen.offsetDimesion != OffScreenDimension.None)
            {
                int padding = 50;
                if (WindowIsOffScreen.offsetDimesion == OffScreenDimension.Bottom)
                {
                    this.Top = this.Top - WindowIsOffScreen.val - padding;
                }
            }
        }

        public struct Offset
        {
            public OffScreenDimension offsetDimesion;
            public int val;

            public Offset(OffScreenDimension offsetDimesion, int val)
            {
                this.offsetDimesion = offsetDimesion;
                this.val = val;
            }
        }

        // make window draggable
        // TODO: Make interface for draggable window?
        // TODO: Ask Bind how to avoid duplication of this method across all windows
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}