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
 
        public Func<FrameworkElement> GetVisual { get; set; }

        private void OnCloseClick(Object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var visual = GetVisual?.Invoke();
            if (visual != null)
            {
                var location = visual.PointToScreen(new Point(0, -6));
                Left = (visual.ActualWidth - ActualWidth) / 2d + location.X;
                Top = location.Y;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
