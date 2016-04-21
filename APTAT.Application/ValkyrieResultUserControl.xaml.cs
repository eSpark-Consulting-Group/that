using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using APTAT.Application.Models;
using APTAT.Core;
using Telerik.Windows.Controls;

namespace APTAT.Application
{
    public partial class ValkyrieResultUserControl : UserControl
    {
        public ValkyrieResultUserControl()
        {
            InitializeComponent();

            Loaded += (s, e) =>
                {
                    var menu = new RadContextMenu();
                    var item = new MenuItem();
                    item.Header = "Details";
                    menu.IconColumnWidth = 0;
                    item.Click += item_Click;
                    menu.Items.Add(item);
                    RadContextMenu.SetContextMenu(treeList2, menu);
                };
        }

        void item_Click(object sender, RoutedEventArgs e)
        {
            if (treeList2.SelectedItem != null)
            {
                var item = treeList2.SelectedItem as FileResult;
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

                var item = element.DataContext as FileResult;
                if (item != null && ViewModel.SelectedValkyrieItem == item)
                {
                    ValkyrieDetailsWindow details = new ValkyrieDetailsWindow();
                    details.Owner = System.Windows.Application.Current.MainWindow;
                    details.ShowDialog();
                }
            }
        }

    }
}
