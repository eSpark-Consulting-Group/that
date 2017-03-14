using System;
using System.Windows;

namespace FAT.Application
{
    public partial class ScanResultWindow : Window
    {
        public ScanResultWindow()
        {
            InitializeComponent();
        }
 
        private void OnCloseClick(Object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
