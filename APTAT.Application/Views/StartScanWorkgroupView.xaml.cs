using APTAT.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Telerik.Windows.Controls.GridView;
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

        private void OnSelectionChanged(Object sender, SelectionChangeEventArgs e)
        {
            StartScanWorkgroupViewModel viewModel = (StartScanWorkgroupViewModel)DataContext;
            viewModel.CheckedItems = computersGrid.SelectedItems;
        }

      
        private void OnRowLoaded(object sender, RowLoadedEventArgs e)
        {
            if (e.Row is GridViewRow && !(e.Row is GridViewNewRow))
            {
                DeploymentItem item = e.DataElement as DeploymentItem;
                var chk = e.Row.ChildrenOfType<GridViewCell>().ToList();
                foreach (var c in chk)
                {
                    if (item.IsOnline)
                    {
                        c.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        c.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public void ClearPasswordBox()
        {
            password.Password = "";
        }
    }
}
