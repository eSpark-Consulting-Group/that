using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for AgentWindow.xaml
    /// </summary>
    public partial class AgentWindow : Window
    {
        public AgentWindow()
        {
            InitializeComponent();

            SourceInitialized += (x, y) =>
            {
                this.HideMinimizeAndMaximizeButtons();
            };

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
