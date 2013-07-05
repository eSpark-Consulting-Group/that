using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
