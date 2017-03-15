using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace FAT.Application
{
    public partial class ScanResultWindow : Window
    {
        public ScanResultWindow()
        {
            InitializeComponent();
        }
 
        public Func<Visual> GetVisual { get; set; }

        private void OnCloseClick(Object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var location = GetVisual?.Invoke().PointToScreen(new Point(-6, -6));
            if (location.HasValue)
            {
                Left = location.Value.X;
                Top = location.Value.Y;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
