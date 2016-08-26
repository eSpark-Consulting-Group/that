using System;
using System.Windows;
using System.Windows.Input;

namespace DDT.Application.Views
{
    public partial class StopConfirmationWindow : Window
    {
        public StopConfirmationWindow()
        {
            InitializeComponent();
        }
 
        private void OnCloseClick(Object sender, RoutedEventArgs e)
        {
            DialogResult = null;
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
