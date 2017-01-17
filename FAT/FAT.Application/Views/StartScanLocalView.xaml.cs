using FAT.Application.Models;
using System;

using System.Windows.Automation;
using System.Windows.Controls;

using Telerik.Windows;
using Telerik.Windows.Controls;

using System.Linq;
using static FAT.Application.StartScanView;
using System.Windows;

namespace FAT.Application
{
    /// <summary>
    /// Interaction logic for StartScanLocalView.xaml
    /// </summary>
    public partial class StartScanLocalView : UserControl
    {
        public StartScanLocalViewModel ViewModel
        {
            get { return (StartScanLocalViewModel)DataContext; }
        }

        public StartScanLocalView()
        {
            InitializeComponent();
        }

        public RoutedEventHandler QuickScanAction { get; set; }
        public RoutedEventHandler FullScanAction { get; set; }
        public RoutedEventHandler CustomScanAction { get; set; }

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
            var directoryItem = (FolderItem)context.DirectoryItem;

            treeViewItem.ItemsSource = directoryItem.Items;
            treeViewItem.IsLoadOnDemandEnabled = false;
        }

        private void OnItemPrepared(object sender, RadTreeViewItemPreparedEventArgs e)
        {
            e.PreparedItem.Focusable = false;
        }


        private void OnChecked(Object sender, RadRoutedEventArgs e)
        {
            RadTreeViewItem treeViewItem = (RadTreeViewItem)e.OriginalSource;

            if (!((RadTreeViewCheckEventArgs)e).IsUserInitiated && (_checkLevel > -1 && treeViewItem.Level < _checkLevel))
            {
                if (treeViewItem.CheckState != ToggleState.Indeterminate)
                    treeViewItem.CheckState = ToggleState.Indeterminate;
            }
            else
            {
                if (((RadTreeViewCheckEventArgs)e).IsUserInitiated)
                    _checkLevel = -1;

                ViewModel.CheckItemCommand.Execute(treeViewItem.Item);
            }
        }

        private int _checkLevel = -1;
        private void OnPreviewChecked(object sender, RadRoutedEventArgs e)
        {
            RadTreeViewItem treeViewItem = (RadTreeViewItem)e.OriginalSource;

            if (((RadTreeViewCheckEventArgs)e).IsUserInitiated && _checkLevel == -1)
                _checkLevel = treeViewItem.Level;
        }

        private void OnUnchecked(Object sender, RadRoutedEventArgs e)
        {
            RadTreeViewItem treeViewItem = (RadTreeViewItem)e.OriginalSource;
            VerifyParents(treeViewItem);
        }

        private RadTreeViewItem VerifyParents(RadTreeViewItem item)
        {
            if (item == null)
                return null;
            else
                ViewModel.UncheckItemCommand.Execute(item.Item);

            var parent = item.ParentItem;
            if (parent != null && parent.CheckState == ToggleState.Indeterminate)
            {
                if (parent.ChildrenOfType<RadTreeViewItem>().All(_ => _.CheckState == ToggleState.Off))
                    parent.CheckState = ToggleState.Off;

                return VerifyParents(parent);
            }

            return null;
        }

        private ToolTip _tooltip = new ToolTip();
        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var button = sender as Button;

            if (button.GetValue(ToolTipHelper.ToolTipProperty) == null)
                button.SetValue(ToolTipHelper.ToolTipProperty, _tooltip);
        }

        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var button = sender as Button;

            button.SetValue(ToolTipHelper.ToolTipProperty, null);
        }

        private void OnFullClick(Object sender, RoutedEventArgs e)
        {
            FullScanAction?.Invoke(sender, e);
        }

        private void OnQuickClick(Object sender, RoutedEventArgs e)
        {
            QuickScanAction?.Invoke(sender, e);
        }

        private void OnCustomClick(Object sender, RoutedEventArgs e)
        {
            CustomScanAction?.Invoke(sender, e);
        }

    }
}
