
using System.Reflection;
using System.Windows;

namespace DDT.Application
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();

            txtProductName.Text = "Auto Discovery and Deployment Tool";
            txtProductVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
