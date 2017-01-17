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

namespace FAT.Application
{
    /// <summary>
    /// Interaction logic for ThreatDetailsWindow.xaml
    /// </summary>
    public partial class ValkyrieDetailsWindow : Window
    {
        public ValkyrieDetailsWindow()
        {
            InitializeComponent();


        }

        private void OnBackClick(object sender, EventArgs e)
        {
            DialogResult = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            DataContext = false;
            DialogResult = true;
        }

 
    }
}
