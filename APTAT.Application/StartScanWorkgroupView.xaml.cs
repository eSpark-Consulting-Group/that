using APTAT.Application.Models;
using System;
using System.Collections.Generic;
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
using Telerik.Windows.Controls;

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

        private void OnPasswordChanged(Object sender, RoutedEventArgs e)
        {
            StartScanWorkgroupViewModel viewModel = (StartScanWorkgroupViewModel)DataContext;
            viewModel.Password = password.Password;
        }

        private void OnSelectionChanged(Object sender, SelectionChangeEventArgs e)
        {
            StartScanWorkgroupViewModel viewModel = (StartScanWorkgroupViewModel)DataContext;
            viewModel.CheckedItems = computersGrid.SelectedItems;
        }
    }
}
