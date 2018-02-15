using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using UFH.Application.Models;
using Telerik.Windows.Controls;
using UFH.Application.Extensions;
using System.Diagnostics;
using System.Windows.Navigation;

namespace UFH.Application
{
    public partial class PreviousScanResults : UserControl
    {
        public PreviousScanResults()
        {
            InitializeComponent();

            DataContext = App.Current.MainWindow.DataContext;
        }

        private MainViewModel ViewModel
        {
            get { return DataContext as MainViewModel; }
        }

        private void treeList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            var contentElement = e.OriginalSource as FrameworkContentElement;

            var operation = (element?.DataContext as IScanOperation) ?? (contentElement?.DataContext as IScanOperation);
            if (operation != null)
                treeList.SelectedItem = operation;
        }

        private void treeList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            var contentElement = e.OriginalSource as FrameworkContentElement;

            var operation = (element?.DataContext as IScanOperation) ?? (contentElement?.DataContext as IScanOperation);
            if (operation != null)
                ViewModel.OpenPreviousScanDetailsCommand.Execute(operation);
        }

        private void item_Click(object sender, RoutedEventArgs e)
        {
            var operation = treeList.SelectedItem as IScanOperation;
            ViewModel.OpenPreviousScanDetailsCommand.Execute(operation);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (RadContextMenu.GetContextMenu(treeList) != null)
                return;

            var menu = new RadContextMenu();
            menu.IconColumnWidth = 0;
            menu.Width = 250;
            AutomationProperties.SetAutomationId(menu, "ScanResultsByComputersContextMenu");

            var item = new RadMenuItem();
            item.CreateDetailsHeader();
            item.Command = ViewModel.OpenPreviousScanDetailsCommand;
            item.Click += item_Click;
            AutomationProperties.SetAutomationId(item, "DetailsContextMenuItem");
            item.SetVisibilityBinding();
            menu.Items.Add(item);

            RadContextMenu.SetContextMenu(treeList, menu);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
