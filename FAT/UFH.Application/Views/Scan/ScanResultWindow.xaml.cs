using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace UFH.Application
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

        protected async override void OnActivated(EventArgs e)
        {
            if (App.Current.MainWindow.WindowState == WindowState.Minimized)
            {
                await Dispatcher.BeginInvoke((Action)delegate
                {
                    App.Current.MainWindow.WindowState = WindowState.Normal;
                    App.Current.MainWindow.Activate();
                    
                }, DispatcherPriority.ContextIdle, null);          
            }

            var visual = GetVisual?.Invoke();
            if (visual != null)
            {
                ClosePopups();

                var location = visual.PointToScreen(new Point(0, -6));

                Left = (visual.ActualWidth - ActualWidth) / 2d + location.X;
                Top = location.Y;
            }

            base.OnActivated(e);
        }

        private void ClosePopups()
        {
            var popup = PresentationSource.CurrentSources.OfType<HwndSource>().
                            Select(h => h.RootVisual).OfType<FrameworkElement>().
                            Select(f => f.Parent).OfType<Popup>().
                            FirstOrDefault(p => p.IsOpen);
            if (popup != null)
                popup.IsOpen = false;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
