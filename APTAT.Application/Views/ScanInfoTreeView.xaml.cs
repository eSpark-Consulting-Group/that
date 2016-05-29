using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using APTAT.Application.Models;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for ScanInfoTreeView.xaml
    /// </summary>
    public partial class ScanInfoTreeView : UserControl
    {
        public ScanInfoTreeView()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var menu = new RadContextMenu();
                menu.IconColumnWidth = 0;
                menu.Width = 160;
                menu.Opened += menu_Opened;

                var item = new RadMenuItem();
                item.Header = "Details";
                item.Command = ViewModel.SelectCommand;
                item.Click += item_Click;
                menu.Items.Add(item);

                RadContextMenu.SetContextMenu(treeList, menu);
            };
        }

        void menu_Opened(object sender, RoutedEventArgs e)
        {
            RadContextMenu menu = (RadContextMenu)sender;
            var cell = menu.GetClickedElement<GridViewCell>();
            if (cell != null)
            {
                cell.IsSelected = true;
                cell.IsCurrent = true;

                treeList.SelectedItem = cell.DataContext is ThreatInfoItem && !((ThreatInfoItem)cell.DataContext).IsRootItem ? cell.DataContext : null;
            }
        }

        void item_Click(object sender, RoutedEventArgs e)
        {
            if (treeList.SelectedItem != null)
            {
                var item = treeList.SelectedItem as ThreatInfoItem;
                if (ViewModel.SelectedItem != null && !ViewModel.SelectedItem.IsRootItem && ViewModel.SelectedItem == item)
                {
                    ThreatDetailsWindow details = new ThreatDetailsWindow();
                    details.Owner = System.Windows.Application.Current.MainWindow;
                    details.ShowDialog();
                }
            }
        }

        public MainViewViewModel ViewModel
        {
            get { return this.DataContext as MainViewViewModel; }     
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
                if (ViewModel.SelectedItem != null && !ViewModel.SelectedItem.IsRootItem && ViewModel.SelectedItem == item)
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
