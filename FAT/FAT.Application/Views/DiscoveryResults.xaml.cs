
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using FAT.Application.Messages;
using FAT.Application.Models;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace FAT.Application
{
    public partial class DiscoveryResults : UserControl
    {
        public DiscoveryResults()
        {
            InitializeComponent();

            DataContext = App.Current.MainWindow.DataContext;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                BindingOperations.SetBinding(this, SelectAllProperty, new Binding("SelectDiscoveryResults"));
            }
        }

        #region SelectAllProperty (DependencyProperty)

        public bool SelectAll
        {
            get { return (bool)GetValue(SelectAllProperty); }
            set { SetValue(SelectAllProperty, value); }
        }

        public static readonly DependencyProperty SelectAllProperty =
            DependencyProperty.Register("SelectAll", typeof(bool), typeof(DiscoveryResults),
            new PropertyMetadata(false, OnSelectAllChanged));

        private static void OnSelectAllChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as DiscoveryResults;
            control.treeList.SelectAll();
        }

        #endregion

        private MainViewViewModel ViewModel
        {
            get { return DataContext as MainViewViewModel; }
        }

        private void treeList_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            foreach (DeploymentTarget item in e.AddedItems)
                ViewModel.SelectedDiscoveryResults.Add(item);
            foreach (DeploymentTarget item in e.RemovedItems)
                ViewModel.SelectedDiscoveryResults.Remove(item);
            ViewModel.SelectedDiscoveryResults = ViewModel.SelectedDiscoveryResults;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            AppMessages.FilterMessage.Send(MessageTypes.FilterTarget, _ => _.Target = treeList);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {     
            var element = e.OriginalSource as FrameworkElement;
            var cell = element.GetVisualParent<GridViewCell>();
            if (cell != null && cell.IsSelected && cell.IsMouseOver && !cell.ParentRow.IsSelected)
                cell.ParentRow.IsSelected = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            var cell = element.GetVisualParent<GridViewCell>();
            if (cell != null && !cell.IsSelected && cell.IsMouseOver && cell.ParentRow.IsSelected)
                cell.ParentRow.IsSelected = false;
        }
    }
}
