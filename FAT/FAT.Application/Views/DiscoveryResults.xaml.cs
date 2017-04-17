
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using FAT.Application.Messages;
using FAT.Application.Models;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using System.Linq;

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
                BindingOperations.SetBinding(this, SelectAllProperty, new Binding("SelectDiscoveryResults") { Mode = BindingMode.TwoWay });
            }
        }

        private bool _lockSelections;

        private MainViewModel ViewModel
        {
            get { return DataContext as MainViewModel; }
        }

        #region SelectAllProperty (DependencyProperty)

        public bool? SelectAll
        {
            get { return (bool?)GetValue(SelectAllProperty); }
            set { SetValue(SelectAllProperty, value); }
        }

        public static readonly DependencyProperty SelectAllProperty =
            DependencyProperty.Register("SelectAll", typeof(bool?), typeof(DiscoveryResults),
            new PropertyMetadata(false, OnSelectAllChanged));

        private static void OnSelectAllChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as DiscoveryResults;
            var newValue = e.NewValue as bool?;
            if (newValue.HasValue)
            {
                control._lockSelections = true;
                control.treeList.SelectedItems.Clear();
                if (newValue.Value)
                {    
                    foreach (var result in control.ViewModel.DiscoveryResults)
                    {
                        result.IsSelected = true;
                        control.treeList.SelectedItems.Add(result);
                    }
                }
                else
                {
                    foreach (var result in control.ViewModel.DiscoveryResults)
                        result.IsSelected = false;
                }
                control._lockSelections = false;
            }
        }

        #endregion

        private void treeList_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            if (_lockSelections)
                return;

            foreach (DeploymentTarget item in e.AddedItems)
                item.IsSelected = true;

            foreach (DeploymentTarget item in e.RemovedItems)
                item.IsSelected = false;

            var selectedCount = ViewModel.DiscoveryResults.Count(_ => _.IsSelected);
            SelectAll = selectedCount == 0 ? false : (selectedCount == ViewModel.DiscoveryResults.Count ? true : (bool?)null);
            BindingOperations.GetBindingExpression(this, SelectAllProperty).UpdateSource();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            AppMessages.FilterMessage.Send(MessageTypes.FilterTarget, _ => _.Target = treeList);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var element = e.OriginalSource as CheckBox;
            var cell = element.GetVisualParent<GridViewCell>();
            if (cell != null && cell.IsMouseOver)
                cell.ParentRow.IsSelected = element.IsChecked.GetValueOrDefault();
        }
    }
}
