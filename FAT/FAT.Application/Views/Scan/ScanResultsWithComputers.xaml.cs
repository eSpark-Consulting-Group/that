using System;
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
            get { return this.DataContext as MainViewViewModel; }
        }

        private void menu_Opened(object sender, RoutedEventArgs e)
        {
            RadContextMenu menu = (RadContextMenu)sender;
            var cell = menu.GetClickedElement<GridViewCell>();
            if (cell != null && !cell.IsSelected)
            {
                cell.IsSelected = true;
                cell.IsCurrent = true;

                treeList.SelectedItem = cell.DataContext;
            }
            ViewModel.SelectedItems = ViewModel.SelectedItems;
        }

        private void item_Click(object sender, RoutedEventArgs e)
        {
            if (treeList.SelectedItem != null)
            {
                var item = treeList.SelectedItem as ThreatInfoItem;
                if (ViewModel.SelectedItems != null && ViewModel.SelectedItems.Count == 1 &&
                    !ViewModel.SelectedItems[0].IsComptuer && ViewModel.SelectedItems[0] == item)
                {
                    ThreatDetailsWindow details = new ThreatDetailsWindow();
                    details.Owner = System.Windows.Application.Current.MainWindow;
                    details.ShowDialog();
                }
            }
        }

        private ThreatInfoItem GetDataBoundItem(FrameworkElement element)
        {
            if (element == null)
            {
                return null;
            }

            if (element.Parent == null)
            {
                return null;
            }

            FrameworkElement parent = (FrameworkElement)element.Parent;
            if (parent == null)
            {
                return null;
            }

            parent = (FrameworkElement)parent.Parent;
            if (parent == null)
            {
                return null;
            }

            return parent.DataContext as ThreatInfoItem;
        }

        private void OnTreeDoubleClick(Object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource != null)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                if (element == null)
                {
                    return;
                }

                ThreatInfoItem item = GetDataBoundItem(element);
                if (ViewModel.SelectedItems != null && ViewModel.SelectedItems.Count == 1 &&
                    !ViewModel.SelectedItems[0].IsComptuer && ViewModel.SelectedItems[0] == item)
                {
                    ViewModel.SelectedItem = item;
                    ThreatDetailsWindow details = new ThreatDetailsWindow();
                    details.Owner = System.Windows.Application.Current.MainWindow;
                    details.ShowDialog();
                }
            }
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
