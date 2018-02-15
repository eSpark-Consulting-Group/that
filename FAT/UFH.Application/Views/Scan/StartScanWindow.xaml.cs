using System;
using System.Windows;
using System.Windows.Input;

namespace UFH.Application
{
    /// <summary>
    /// Interaction logic for StartScanWindow.xaml
    /// </summary>
    public partial class StartScanWindow
    {
        public StartScanWindow()
        {
            InitializeComponent();
        }

        public bool IsQuickScan { get; private set; }

        private void OnFinishClick(Object sender, StartScanEventArgs e)
        {
            IsQuickScan = e.IsQuickScan;
            DialogResult = true;
        }

        private void OnCancelClick(Object sender, EventArgs e)
        {
            DialogResult = false;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
