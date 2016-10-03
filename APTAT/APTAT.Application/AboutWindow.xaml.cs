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


            txtProductName.Text = "Unknown File Hunter";
            txtProductVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
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
