using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for BannerUserControl.xaml
    /// </summary>
    public partial class BannerUserControl : UserControl
    {
        private string _currentLink; 

        public BannerUserControl()
        {
            InitializeComponent();
            _currentLink = "CDMLink";
        }

        private void SetBackground(string imagePath)
        {
            var imageBrush = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), imagePath)));
            LayoutGrid.Background = imageBrush;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var resource = TryFindResource(_currentLink);
            Process.Start(new ProcessStartInfo(resource.ToString()));
            e.Handled = true;
         
        }
    }
}
