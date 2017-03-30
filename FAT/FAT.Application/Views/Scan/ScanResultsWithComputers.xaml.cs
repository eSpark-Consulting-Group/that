
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using FAT.Application.Models;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using FAT.Application.Extensions;
using FAT.Application.Messages;

namespace FAT.Application
{
    public partial class ScanResultsWithComputers : UserControl
    {
        public ScanResultsWithComputers()
        {
            InitializeComponent();

            DataContext = App.Current.MainWindow.DataContext;
        }

        private MainViewViewModel ViewModel
        {
            get { return DataContext as MainViewViewModel; }
        }

        private void treeList_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            foreach (IThreatInfoItem item in e.AddedItems)
                ViewModel.SelectedItems.Add(item);
            foreach (IThreatInfoItem item in e.RemovedItems)
                ViewModel.SelectedItems.Remove(item);
            ViewModel.SelectedItems = ViewModel.SelectedItems;
        }

        private void treeList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;

            var item = element?.DataContext as IThreatInfoItem;
            if (item != null)
            {
                treeList.SelectedItem = null;
                ViewModel.SelectedItems.Clear();
                treeList.SelectedItem = item;
            }
        }

        private void menu_Opened(object sender, RoutedEventArgs e)
        {
            var menu = sender as RadContextMenu;
            if (!menu.Items.OfType<RadMenuItem>().Any(_ => _.Command.CanExecute(null)))
                menu.IsOpen = false;
        }

        private void treeList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext as IThreatInfoItem;
            if (item != null && !item.IsComputer)
                item_Click(sender, e);
        }

        private void item_Click(object sender, RoutedEventArgs e)
        {
            var details = new ThreatDetailsWindow();
            details.Owner = App.Current.MainWindow;
            details.ShowDialog();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (RadContextMenu.GetContextMenu(treeList) != null)
                return;

            var menu = new RadContextMenu();
            menu.IconColumnWidth = 0;
            menu.Width = 250;
            menu.Opened += menu_Opened;
            AutomationProperties.SetAutomationId(menu, "ScanResultsByComputersContextMenu");

            var item = new RadMenuItem();
            item.CreateDetailsHeader();
            item.Command = ViewModel.SelectCommand;
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

            AppMessages.FilterMessage.Send(MessageTypes.FilterTarget, _ => _.Target = treeList);
        }

        private void treeList_DistinctValuesLoading(object sender, GridViewDistinctValuesLoadingEventArgs e)
        {
            e.ItemsSource = new[]
            {
                new ScanStatus(ScanStatusEnum.InProgress),
                new ScanStatus(ScanStatusEnum.Canceled),
                new ScanStatus(ScanStatusEnum.Completed),
                new ScanStatus(ScanStatusEnum.Failed),
                new ScanStatus(ScanStatusEnum.Offline)
            };
        }

        private void treeList_RowIsExpandedChanged(object sender, RowEventArgs e)
        {
            treeList.ScrollIntoView(e.Row.DataContext);
        }
    }
}
