using APTAT.Application.Extensions;
using System;
using System.Windows;
using System.Windows.Input;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for ConfirmationWindow.xaml
    /// </summary>
    public partial class ConfirmationWindow : Window
    {
        public ConfirmationWindow()
        {
            InitializeComponent();

            Loaded += ConfirmationWindow_Loaded;
        }

        private void ConfirmationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            no.DelayedFocus();
        }

        private void OnCloseClick(Object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void YesClick(Object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void NoClick(Object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
