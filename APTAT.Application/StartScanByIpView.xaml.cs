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
using Telerik.Windows.Controls.GridView;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for StartScanByIpView.xaml
    /// </summary>
    public partial class StartScanByIpView : UserControl
    {
        public StartScanByIpView()
        {
            InitializeComponent();

      
        }

        private void OnPasswordChanged(Object sender, RoutedEventArgs e)
        {
            StartScanByIpViewModel viewModel = (StartScanByIpViewModel)DataContext;
            viewModel.Password = password.Password;
        }

        private void OnSelectionChanged(Object sender, SelectionChangeEventArgs e)
        {
           
            StartScanByIpViewModel viewModel = (StartScanByIpViewModel)DataContext;
            viewModel.CheckedItems = computersGrid.SelectedItems;
        }


        private void OnSelectionChanging(Object sender, GridViewSelectedCellsChangingEventArgs e)
        {
        }
    }
}
