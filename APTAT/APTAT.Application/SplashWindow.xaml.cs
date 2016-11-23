using System;
using System.Reflection;
using System.Windows;


namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
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