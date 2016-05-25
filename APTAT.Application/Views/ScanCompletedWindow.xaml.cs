using System.Configuration;
using APTAT.Application.Models;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for ScanCompletedWindow.xaml
    /// </summary>
    public partial class ScanCompletedWindow
    {
        public ScanCompletedWindow()
        {
            InitializeComponent();

            SourceInitialized += (x, y) =>
            {
                this.HideMinimizeAndMaximizeButtons();
            };
        }

        public ScanCompletedWindow(MainViewViewModel currentViewModel)
            :this()
        {
            DataContext = currentViewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((bool) ((FrameworkElement) e.Source).Tag)
                DialogResult = false;
            else
                DialogResult = true;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {

            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
