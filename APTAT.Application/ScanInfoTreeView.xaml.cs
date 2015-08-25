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
using APTAT.Application.Models;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for ScanInfoTreeView.xaml
    /// </summary>
    public partial class ScanInfoTreeView : UserControl
    {
        public ScanInfoTreeView(MainViewViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
        }

        private MainViewViewModel _viewModel;

        private void treeList_LayoutUpdated(object sender, EventArgs e)
        {

            var rows = this.treeList.ChildrenOfType<GridViewRow>();
            foreach (GridViewRow row in rows)
            {
                if (row != null)
                {
                    ThreatInfoItem threatInfo = row.DataContext as ThreatInfoItem;
                    if (threatInfo != null)
                    {
                        row.IsExpandable = threatInfo.TempItems != null && threatInfo.TempItems.Count > 0 && threatInfo.ScanStage == ScanStage.OperationCompleted;
                    }
                }
            }

        }

        private void treeList_Filtered(object sender, GridViewFilteredEventArgs e)
        {
            treeList.CollapseAllHierarchyItems();

        }

        private void treeList_RowIsExpandedChanged(object sender, RowEventArgs e)
        {
            var row = e.Row as GridViewRow;
            if (row.IsExpanded)
            {
                if (row != null)
                {
                    ThreatInfoItem item = ((ThreatInfoItem)row.DataContext);

                    if (item.ScanStage == ScanStage.OperationCompleted)
                    {
                        if (item.TempItems.Count > 0)
                        {
                            item.Items = item.TempItems;

                            //if (!IsSearchPerformed)
                            //{
                            foreach (ThreatInfoItem subItem in item.Items)
                            {
                                subItem.IsCompletedSubItem = true;
                            }
                            //}
                            //else
                            //    IsSearchPerformed = false;
                        }
                    }
                }
            }
            else
            {
                ThreatInfoItem item = ((ThreatInfoItem)row.DataContext);
                if (item.Items != null)
                {
                    foreach (ThreatInfoItem subItem in item.Items)
                    {
                        subItem.IsCompletedSubItem = false;
                    }
                }

                //item.Items = null;
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
                if (_viewModel.SelectedItem != null && _viewModel.SelectedItem.IsRootItem == false && _viewModel.SelectedItem == item)
                {
                    ThreatDetailsWindow details = new ThreatDetailsWindow();
                    details.Owner = System.Windows.Application.Current.MainWindow;
                    details.ShowDialog();
                }
            }
        }

        private void OnRadGridViewDistinctValuesLoading(Object sender, GridViewDistinctValuesLoadingEventArgs e)
        {
            e.ItemsSource = new[]
            {
                new ScanStatus(ScanStatusEnum.Cancelled),
                new ScanStatus(ScanStatusEnum.Completed),
                new ScanStatus(ScanStatusEnum.InProgress),
                new ScanStatus(ScanStatusEnum.Failed),
                new ScanStatus(ScanStatusEnum.Offline)
            };
        }
    }
}
