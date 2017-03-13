
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using FAT.Application.Models;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using FAT.Application.Extensions;
using System.Diagnostics;
using System.Windows.Navigation;

namespace FAT.Application
{
    public partial class PreviousScanResults : UserControl
    {
        public PreviousScanResults()
        {
            InitializeComponent();

            DataContext = App.Current.MainWindow.DataContext;
        }

        private MainViewViewModel ViewModel
        {
            get { return DataContext as MainViewViewModel; }
        }

        private void treeList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            var cell = element.GetVisualParent<GridViewCell>();
            if (cell != null && !cell.IsSelected)
            {
                cell.IsSelected = true;
                cell.IsCurrent = true;

                treeList.SelectedItem = cell.DataContext;
            }
        }

        private void treeList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            var cell = element.GetVisualParent<GridViewCell>();
            if (cell != null)
            {
                var operation = cell.DataContext as IScanOperation;
                ViewModel.OpenPreviousScanDetailsCommand.Execute(operation);
            }
        }

        private void item_Click(object sender, RoutedEventArgs e)
        {
            var operation = treeList.SelectedItem as IScanOperation;
            ViewModel.OpenPreviousScanDetailsCommand.Execute(operation);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (RadContextMenu.GetContextMenu(treeList) != null)
                return;

            var menu = new RadContextMenu();
            menu.IconColumnWidth = 0;
            menu.Width = 250;
            //menu.Opened += menu_Opened;
            AutomationProperties.SetAutomationId(menu, "ScanResultsByComputersContextMenu");

            var item = new RadMenuItem();
            item.CreateDetailsHeader();
            item.Command = ViewModel.OpenPreviousScanDetailsCommand;
            item.Click += item_Click;
            AutomationProperties.SetAutomationId(item, "DetailsContextMenuItem");
            item.SetVisibilityBinding();
            menu.Items.Add(item);

            /*item = new RadMenuItem();
            item.CreateRestartScanHeader();
            item.Command = ViewModel.RestartComputerScanCommand;
            item.SetVisibilityBinding();
            menu.Items.Add(item);

            item = new RadMenuItem();
            item.CreateRestartScanWithCredentialsHeader();
            item.Command = ViewModel.RestartComputerScanWithCredentialsCommand;
            item.SetVisibilityBinding();
            menu.Items.Add(item);

            item = new RadMenuItem();
            item.CreateStopScanHeader();
            item.Command = ViewModel.StopComputerScanCommand;
            item.SetVisibilityBinding();
            menu.Items.Add(item);*/

            RadContextMenu.SetContextMenu(treeList, menu);

            //AppMessages.FilterMessage.Send(MessageTypes.FilterTarget, _ => _.Target = treeList);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
