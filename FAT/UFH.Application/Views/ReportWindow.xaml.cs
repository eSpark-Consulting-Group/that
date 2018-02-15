
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace UFH.Application
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow 
    {
        public ReportWindow()
        {
            InitializeComponent();
        }

        #region HeaderProperty (DependencyProperty)
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ReportWindow),
            new PropertyMetadata("Report Preview"));

        #endregion

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnResizeClick(object sender, RoutedEventArgs e) => SwitchState(sender as Button);



        private void SwitchState(Button sender)
        {
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

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

    }
}
