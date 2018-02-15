
using System.Reflection;
using System.Windows;

namespace UFH.Application
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();

            txtProductName.Text = "Unknown File Hunter";
            txtProductVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}