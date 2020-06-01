
using System.Reflection;
using System.Windows;

namespace UFH.Application
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();

            txtProductName.Text = "ZeroDayMalwareHunter";
            txtProductVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}