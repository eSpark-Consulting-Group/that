using System;
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
        private bool restoreIfMove = false;

        public MainWindow()
        {
            InitializeComponent();

            MouseLeftButtonDown += MainWindow_MouseDown;
        

            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            StateChanged += OnStateChanged;
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
        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized) return;
            DragMove();
        }
        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if ((ResizeMode == ResizeMode.CanResize) ||
                    (ResizeMode == ResizeMode.CanResizeWithGrip))
                {
                    SwitchState();
                }
            }
            else
            {
                if (WindowState == WindowState.Maximized)
                {
                    restoreIfMove = true;
                    DragMove();
                }
            }
        }
        private void OnResizeClick(object sender, RoutedEventArgs e)
        {
            SwitchState();
        }

        private void OnStateChanged(object sender, EventArgs eventArgs)
        {
            var maxStyle = FindResource("MaximizeButton") as Style;
            var restoreStyle = FindResource("RestoreWindowButton") as Style;
            if (WindowState == WindowState.Maximized)
            {
                ResizeButton.Style = restoreStyle;
            }
            else
            {
                ResizeButton.Style = maxStyle;
            }
        }
        private void SwitchState()
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    {
                        WindowState = WindowState.Maximized;
                        break;
                    }
                case WindowState.Maximized:
                    {
                        WindowState = WindowState.Normal;
                        break;
                    }
            }
        }

        private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (restoreIfMove)
            {
                restoreIfMove = false;
               
                var mouseX = e.GetPosition(this.Border).X;
                var width = RestoreBounds.Width;
                var x = mouseX - width / 2;

                if (x < 0)
                {
                    x = 0;
                }
                else
                if (x + width > SystemParameters.VirtualScreenWidth)
                {
                    x = SystemParameters.VirtualScreenWidth - width;
                }
                var leftBeforeNormal = GetWindowLeft(this);
                Left = leftBeforeNormal + x;
                WindowState = WindowState.Normal;
                Top = 0;
                DragMove();
            }
        }
        private double GetWindowLeft(Window window)
        {
            if (window.WindowState == WindowState.Maximized)
            {
                var leftField = typeof(Window).GetField("_actualLeft", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return (double)leftField.GetValue(window);
            }
            else
                return window.Left;
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            restoreIfMove = false;
        }
    }
}
