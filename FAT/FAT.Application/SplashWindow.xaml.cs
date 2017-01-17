using System;
using System.Reflection;
using System.Windows;


namespace FAT.Application
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();

            txtProductName.Text = "Forensic Analysis Tool";
            txtProductVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}