using APTAT.Application.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using APTAT.Application.Extensions;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for StartScanWorkgroupView.xaml
    /// </summary>
    public partial class StartScanWorkgroupView : UserControl
    {
        public StartScanWorkgroupView()
        {
            InitializeComponent();
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {            
            PortValidationExtensions.ValidateInput(sender, e);       
        }
        private void OnPasswordChanged(Object sender, RoutedEventArgs e)
        {
            StartScanWorkgroupViewModel viewModel = (StartScanWorkgroupViewModel)DataContext;
            viewModel.Password = password.Password;
        }

        

        public void ClearPasswordBox()
        {
            password.Password = "";
        }
    }
}
