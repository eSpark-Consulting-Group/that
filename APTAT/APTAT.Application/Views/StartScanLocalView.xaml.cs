using APTAT.Application.Models;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows;
using Telerik.Windows.Controls;
using APTAT.Application.Extensions;
using System.Linq;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for StartScanDomainView.xaml
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

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            PortValidationExtensions.ValidateInput(sender, e);
        }

        #region Danger! COT-611 fix
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

                ViewModel.UncheckItemCommand.Execute(treeViewItem.Item);
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
            if (parent != null && parent.CheckState == ToggleState.Indeterminate && 
                parent.Items.SourceCollection.OfType<FolderItem>().All(_ => !_.IsSelected.GetValueOrDefault()))
            {
                parent.CheckState = ToggleState.Off;
                return VerifyParents(parent.ParentItem);
            }

            return null;
        }
        #endregion
    }
}
