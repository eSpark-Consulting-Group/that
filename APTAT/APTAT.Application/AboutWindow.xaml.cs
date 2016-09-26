using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            SourceInitialized += (x, y) =>
            {
                this.HideMinimizeAndMaximizeButtons();
            };
            txtProductName.Text = "Unknown File Hunter";
            txtProductVersion.Text = "2.0";// Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void Window_MouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
