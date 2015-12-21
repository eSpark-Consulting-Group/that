using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainView _mainView;

        public MainWindow()
        {
            InitializeComponent();

            DispatcherHelper.Current = Dispatcher;

            MouseLeftButtonDown += MainWindow_MouseDown;

            SourceInitialized += (x, y) =>
            {
                this.HideMinimizeAndMaximizeButtons();
            };            

            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(TitleProperty, typeof(Window));
            if (dpd != null)
            {
                dpd.AddValueChanged(this, delegate
                {
                    if (txtTitle != null)
                    {
                        txtTitle.Text = Title;
                    }
                });
            }
            _mainView = (MainView) this.MainFrame.Content;
            
            EulaWindow win = new EulaWindow
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Height = this.Height,
                Width =  this.Width
            };
            if(win.ShowDialog() == true) this.Visibility = Visibility.Visible;
        }
        

        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
                WindowState = WindowState.Normal;
                DragMove();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            if(_mainView == null) return;
            
            if (_mainView.IsScanInProgress)
            {
                ConfirmationWindow confirmation = new ConfirmationWindow
                {
                    Owner = System.Windows.Application.Current.MainWindow
                };

                if (confirmation.ShowDialog() == true)
                {
                    Close();
                }
            }
            else
            {
                Close();
            }
        }

        private void OnMinimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnResizeClick(object sender, RoutedEventArgs e)
        {
            var maxStyle = FindResource("MaximizeButton") as Style;
            var restoreStyle = FindResource("RestoreWindowButton") as Style;
            
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                ResizeButton.Style = maxStyle;
            }
            else 
            {
                WindowState = WindowState.Maximized;
                ResizeButton.Style = restoreStyle;
            }
        }
    }
}
