using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using APTAT.Application.Models;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using APTAT.Application.Extensions;


namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for ScanResultUserControlByComputers.xaml
    /// </summary>
    public partial class ScanResultUserControlByComputers : UserControl
    {
        public ScanResultUserControlByComputers()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                if (RadContextMenu.GetContextMenu(treeList) != null)
                    return;
                
                var menu = new RadContextMenu();
                menu.IconColumnWidth = 0;
                menu.Width = 160;
                menu.Opened += menu_Opened;
                AutomationProperties.SetAutomationId(menu, "ScanResultsByComputersContextMenu");

                var item = new RadMenuItem();
                item.CreateHeaderDataTemplate("Details");
                item.Command = ViewModel.SelectCommand;
                item.Click += item_Click;
                AutomationProperties.SetAutomationId(item, "DetailsContextMenuItem");
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
                    ViewModel.SelectedItem = item;
                    ThreatDetailsWindow details = new ThreatDetailsWindow();
                    details.Owner = System.Windows.Application.Current.MainWindow;
                    details.ShowDialog();
                }
            }
        }



    }
}
