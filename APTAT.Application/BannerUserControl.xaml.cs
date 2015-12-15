using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }

        public void SetAdMode(AdMode mode)
        {
            switch (mode)
            {
                case AdMode.Cesm:
                    _currentLink = "CESMLink";
                    SetBackground("Images/banner_1.png");     
                    break;
                case AdMode.Cdm:
                    _currentLink = "CDMLink";
                    SetBackground("Images/banner_2.png");     
                    break;
                default:
                    throw new ArgumentOutOfRangeException("mode", mode, null);
            }
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

    public enum AdMode
    {
        Cesm,
        Cdm
    }
}
