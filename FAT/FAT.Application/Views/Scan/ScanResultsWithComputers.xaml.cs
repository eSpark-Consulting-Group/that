using System;
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
using System.Collections.Generic;

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

        private void treeList_Sorting(object sender, GridViewSortingEventArgs e)
        {
            if (ViewModel.IsAnyScanInProgress)
            {
                e.Cancel = true;
                return;
            }
        }

        private void treeList_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            foreach (ThreatInfoItem item in e.AddedItems)
                ViewModel.SelectedItems.Add(item);
            foreach (ThreatInfoItem item in e.RemovedItems)
                ViewModel.SelectedItems.Remove(item);
            ViewModel.SelectedItems = ViewModel.SelectedItems;
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

                ViewModel.SelectedItems = new List<ThreatInfoItem>(new[] { cell.DataContext as ThreatInfoItem });
            }
        }

        private void treeList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.SelectedItems.Count == 1 && !ViewModel.SelectedItem.IsComputer)
            {
                var details = new ThreatDetailsWindow();
                details.Owner = App.Current.MainWindow;
                details.ShowDialog();
            }
        }

        private void menu_Opened(object sender, RoutedEventArgs e)
        {
            var menu = sender as RadContextMenu;
            if (!menu.Items.OfType<RadMenuItem>().Any(_ => _.Command.CanExecute(null)))
                menu.IsOpen = false;
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
            item.Command = ViewModel.SelectMultipleCommand;
            item.Click += item_Click;
            AutomationProperties.SetAutomationId(item, "DetailsContextMenuItem");
            item.SetVisibilityBinding();
            menu.Items.Add(item);

            item = new RadMenuItem();
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
            menu.Items.Add(item);

            RadContextMenu.SetContextMenu(treeList, menu);

            AppMessages.FilterMessage.Send(ChangeScreenTypes.FilterTarget, _ => _.Target = treeList);
        }
    }
}
