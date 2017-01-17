using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FAT.Application.Models;

using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using FAT.Application.Extensions;
using FAT.Application.Models.Valkyrie;

namespace FAT.Application
{
    public partial class ValkyrieResultUserControlByComputers : UserControl
    {
        public ValkyrieResultUserControlByComputers()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                if (RadContextMenu.GetContextMenu(treeList2) != null)
                    return;

                var menu = new RadContextMenu();
                menu.IconColumnWidth = 0;
                menu.Width = 160;
                menu.Opened += menu_Opened;

                var item = new RadMenuItem();
                item.CreateHeaderDataTemplate("Details");
                item.Command = ViewModel.SelectValkyrieCommand;
                item.Click += item_Click;
                menu.Items.Add(item);
                RadContextMenu.SetContextMenu(treeList2, menu);
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

                treeList2.SelectedItem = cell.DataContext is ValkyrieFile ? cell.DataContext : null;
            }
        }

        void item_Click(object sender, RoutedEventArgs e)
        {
            if (treeList2.SelectedItem != null)
            {
                var item = treeList2.SelectedItem as ValkyrieFile;
                if (item != null && ViewModel.SelectedValkyrieItem == item)
                {
                    ValkyrieDetailsWindow details = new ValkyrieDetailsWindow();
                    details.Owner = System.Windows.Application.Current.MainWindow;
                    details.ShowDialog();
                }
            }
        }

        public MainViewViewModel ViewModel
        {
            get { return this.DataContext as MainViewViewModel; }
        }

        private void OnTreeDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource != null)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                if (element == null)
                {
                    return;
                }

                var item = element.DataContext as ValkyrieFile;
                if (item != null && ViewModel.SelectedValkyrieItem == item)
                {
                    ViewModel.SelectedValkyrieItem = item;
                    ValkyrieDetailsWindow details = new ValkyrieDetailsWindow();
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

    }
}
