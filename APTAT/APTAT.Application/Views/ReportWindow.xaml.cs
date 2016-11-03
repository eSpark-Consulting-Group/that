
using System.Windows;

using System.Windows.Input;


namespace APTAT.Application
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

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

    }
}
