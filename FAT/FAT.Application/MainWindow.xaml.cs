
using FAT.Application.Models;
using APTAT.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FAT.Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = CompositionHelper.Instance.Resolve<MainViewModel>();

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            var _mainView = MainFrame.Content as MainView;

            if (_mainView != null && (_mainView.IsScanInProgress || _mainView.IsDiscoveryInProgress))
            {
                ConfirmationWindow confirmation = new ConfirmationWindow
                {
                    Owner = System.Windows.Application.Current.MainWindow,
                    Title = _mainView.IsScanInProgress ? "Scan" : "Discovery"
                };

                if (confirmation.ShowDialog() == true)
                {
                    _mainView.OnAppClose(App.Current.Shutdown);
                }
            }
            else
            {
                _mainView.OnAppClose(App.Current.Shutdown);
            }
        }

        private void OnResizeClick(object sender, RoutedEventArgs e) => SwitchState(sender as Button);

        private void OnMinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void SwitchState(Button sender)
        {
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            switch (WindowState)
            {
                case WindowState.Normal:
                    {
                        WindowState = WindowState.Maximized;
                        var restoreStyle = FindResource("RestoreWindowButton") as Style;
                        sender.Style = restoreStyle;
                        sender.ToolTip = "Restore";
                        break;
                    }
                case WindowState.Maximized:
                    {
                        WindowState = WindowState.Normal;
                        var maxStyle = FindResource("MaximizeButton") as Style;
                        sender.Style = maxStyle;
                        sender.ToolTip = "Maximize";
                        break;
                    }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (WindowState == WindowState.Maximized) return;
            DragMove();
        }
    }
}
