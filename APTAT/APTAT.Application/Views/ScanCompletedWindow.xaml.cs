using APTAT.Application.Extensions;
using APTAT.Application.Models;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for ScanCompletedWindow.xaml
    /// </summary>
    public partial class ScanCompletedWindow
    {
        public ScanCompletedWindow()
        {
            InitializeComponent();

            Loaded += ScanCompletedWindow_Loaded;
        }

        private void ScanCompletedWindow_Loaded(object sender, RoutedEventArgs e)
        {
            /*Dispatcher.BeginInvoke((Action)delegate
            {
                if (ok.Visibility == Visibility.Visible)
                    ok.Focus();
                else
                    yes.DelayedFocus();
            });*/
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = (bool)((FrameworkElement)e.Source).Tag;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {

            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
