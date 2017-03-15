using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var location = Owner.PointToScreen(new Point(-3, -3));
            Left = location.X + 33;
            Top = location.Y + 280;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
