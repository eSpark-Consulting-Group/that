
using System.Windows.Controls;
using System.Windows.Input;
using APTAT.Application.Extensions;
using Telerik.Windows;
using System;
using Telerik.Windows.Controls;
using System.Windows.Automation;
using APTAT.Application.Models;
using System.Windows;

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

        public StartScanWorkgroupViewModel ViewModel
        {
            get { return (StartScanWorkgroupViewModel)DataContext; }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            text.DelayedFocus();
        }

        private void OnLoadOnDemand(Object sender, RadRoutedEventArgs e)
        {
            RadTreeViewItem clickedItem = e.OriginalSource as RadTreeViewItem;
            ViewModel.ExpandCommand.Execute(new ExpandCommandContext
            {
                DirectoryItem = clickedItem.Item,
                TreeItem = clickedItem,
                Completed = OnExpandCompleted
            });
        }

        private void OnExpandCompleted(ExpandCommandContext context)
        {
            RadTreeViewItem treeViewItem = (RadTreeViewItem)context.TreeItem;
            DirectoryItem directoryItem = (DirectoryItem)context.DirectoryItem;

            treeViewItem.ItemsSource = directoryItem.Items;
            treeViewItem.IsLoadOnDemandEnabled = false;
        }

        private void OnItemPrepared(object sender, RadTreeViewItemPreparedEventArgs e)
        {
            DirectoryItem directoryItem = (DirectoryItem)e.PreparedItem.Item;
            if (directoryItem.Type == DirectoryItemType.Computer)
            {
                e.PreparedItem.IsLoadOnDemandEnabled = false;
            }
            else if (directoryItem.Type == DirectoryItemType.Root)
            {
                e.PreparedItem.IsExpanded = true;
            }
        }

        private void OnChecked(Object sender, RadRoutedEventArgs e)
        {
            RadTreeViewItem treeViewItem = (RadTreeViewItem)e.OriginalSource;
            if (treeViewItem.CheckState == ToggleState.On)
            {
                ViewModel.CheckItemCommand.Execute(treeViewItem.Item);
            }
            else
            {
                ViewModel.UncheckItemCommand.Execute(treeViewItem.Item);
            }

        }

        private void OnUnchecked(Object sender, RadRoutedEventArgs e)
        {
            RadTreeViewItem treeViewItem = (RadTreeViewItem)e.OriginalSource;
            if (treeViewItem.CheckState == ToggleState.On)
            {
                ViewModel.CheckItemCommand.Execute(treeViewItem.Item);
            }
            else
            {
                ViewModel.UncheckItemCommand.Execute(treeViewItem.Item);
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            PortValidationExtensions.ValidateInput(sender, e);
        }

    }
}
