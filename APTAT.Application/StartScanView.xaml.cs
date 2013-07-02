using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
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

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for StartScanView.xaml
    /// </summary>
    public partial class StartScanView
    {
        public event EventHandler CancelClick;
        public event EventHandler FinishClick;

        private readonly StartScanViewModel _viewModel;

        public StartScanView()
        {
            DataContext = _viewModel = new StartScanViewModel();
            InitializeComponent();
            UpdateButtonsVisibility();

            Loaded += StartScanView_Loaded;
        }

        void StartScanView_Loaded(object sender, RoutedEventArgs e)
        {
            txtDomainController.Focus();
        }

        private void OnCancelClick(Object sender, RoutedEventArgs e)
        {
            if(CancelClick != null)
            {
                CancelClick(this, EventArgs.Empty);
            }
        }

        private void OnFinishClick(Object sender, RoutedEventArgs e)
        {
            _viewModel.ChangeStepCommand.Execute(new ChangeStepCommandContext
            {
                Step = 2,
                Completed = ctx =>
                {
                    if(FinishClick != null)
                    {
                        FinishClick(this, EventArgs.Empty);
                    }
                }
            }); 
        }

        private void OnNextClick(Object sender, RoutedEventArgs e)
        {
            txtDomainController.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtAdministrator.GetBindingExpression(TextBox.TextProperty).UpdateSource();

            if( Validation.GetHasError(txtDomainController) == false && 
                Validation.GetHasError(txtAdministrator) == false )
            {
                _viewModel.ChangeStepCommand.Execute(new ChangeStepCommandContext
                {
                    Step = 1,
                    Completed = ctx =>
                                    {
                                        UpdateButtonsVisibility();
                                        domainTree.Focus();

                                    }
                }); 
            }
        }

        private void OnBackClick(Object sender, RoutedEventArgs e)
        {
            _viewModel.ChangeStepCommand.Execute(new ChangeStepCommandContext
            {
                Step = 0,
                Completed = ctx => { UpdateButtonsVisibility(); }
            });
           
        }

        private void UpdateButtonsVisibility()
        {
            if (_viewModel.Step == 0)
            {
                btnBack.Visibility = Visibility.Collapsed;
                btnFinish.Visibility = Visibility.Collapsed;
                btnNext.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Visible;
                panelSelectDomain.Visibility = Visibility.Visible;
                panelSelectComputers.Visibility = Visibility.Collapsed;
                txtSelectComputers.Visibility = Visibility.Collapsed;
                txtSpecifyADInfo.Visibility = Visibility.Visible;
            }
            else if (_viewModel.Step == 1)
            {
                btnBack.Visibility = Visibility.Visible;
                btnFinish.Visibility = Visibility.Visible;
                btnNext.Visibility = Visibility.Collapsed;
                btnCancel.Visibility = Visibility.Visible;
                panelSelectDomain.Visibility = Visibility.Collapsed;
                panelSelectComputers.Visibility = Visibility.Visible;
                txtSelectComputers.Visibility = Visibility.Visible;
                txtSpecifyADInfo.Visibility = Visibility.Collapsed;
            }
        }

        private void OnPasswordChanged(Object sender, RoutedEventArgs e)
        {
            _viewModel.Password = password.Password;
        }

    
        private void OnLoadOnDemand(Object sender, RadRoutedEventArgs e)
        {
            RadTreeViewItem clickedItem = e.OriginalSource as RadTreeViewItem;  
            _viewModel.ExpandCommand.Execute(new ExpandCommandContext
            {
                DirectoryItem = clickedItem.Item,
                TreeItem = clickedItem,
                Completed = OnExpandCompleted
            });
        }

        private void OnExpandCompleted(ExpandCommandContext context)
        {
            RadTreeViewItem treeViewItem = (RadTreeViewItem)context.TreeItem;

            DirectoryItem directoryItem = (DirectoryItem) context.DirectoryItem;

            treeViewItem.ItemsSource = directoryItem.Items;

            treeViewItem.IsLoadOnDemandEnabled = false;
        }

        private void OnItemPrepared(object sender, RadTreeViewItemPreparedEventArgs e)
        {
            DirectoryItem directoryItem = (DirectoryItem)e.PreparedItem.Item;
            if (directoryItem.Type == DirectoryItemType.Computer)
            {
                e.PreparedItem.IsLoadOnDemandEnabled = false;
            }
            else if (directoryItem.Type == DirectoryItemType.Root)
            {
                e.PreparedItem.IsExpanded = true;
            }
        }

        private void OnChecked(Object sender, RadRoutedEventArgs e)
        {
            RadTreeViewItem treeViewItem = (RadTreeViewItem) e.OriginalSource;
            if(treeViewItem.CheckState == ToggleState.On)
            {
                _viewModel.CheckItemCommand.Execute(treeViewItem.Item);
            }
            else
            {
                _viewModel.UncheckItemCommand.Execute(treeViewItem.Item);
            }
            
        }

        private void OnUnchecked(Object sender, RadRoutedEventArgs e)
        {
            RadTreeViewItem treeViewItem = (RadTreeViewItem)e.OriginalSource;
            if (treeViewItem.CheckState == ToggleState.On)
            {
                _viewModel.CheckItemCommand.Execute(treeViewItem.Item);
            }
            else
            {
                _viewModel.UncheckItemCommand.Execute(treeViewItem.Item);
            }
        }

    }

    
}
