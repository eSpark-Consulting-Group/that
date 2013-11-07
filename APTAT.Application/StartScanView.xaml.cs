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
    public class Steps
    {
        public const Int32 InitialStep = 0;
        public const Int32 SettingsStep = 1;
        public const Int32 FinishStep = 2;
    }


    /// <summary>
    /// Interaction logic for StartScanView.xaml
    /// </summary>
    public partial class StartScanView
    {
        public event EventHandler CancelClick;
        public event EventHandler FinishClick;

        //private readonly StartScanViewModel _viewModel;
        private readonly WizardViewModel _scanDomainViewModel;
        private readonly WizardViewModel _scanWorkgroupViewModel;
        private readonly WizardViewModel _scanByIpViewModel;

        private WizardViewModel _currentViewModel;
        ToolTip tooltip;
        public StartScanView()
        {
            InitializeComponent();

            _scanDomainViewModel = new StartScanDomainViewModel();
            _scanWorkgroupViewModel = new StartScanWorkgroupViewModel();
            _scanByIpViewModel = new StartScanByIpViewModel();

            viewScanByIp.DataContext = _scanByIpViewModel;
            viewScanDomain.DataContext = _scanDomainViewModel;
            viewScanWorkgroup.DataContext = _scanWorkgroupViewModel;

            Unloaded += OnUnloaded;

            
      
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
            _currentViewModel.ChangeStepCommand.Execute(new ChangeStepCommandContext
            {
                Step = _currentViewModel.CurrentStepIndex + 1,
                Completed = ctx =>
                {
                }
            });
        }

        private void OnNextClick(Object sender, RoutedEventArgs e)
        {
            _currentViewModel.ChangeStepCommand.Execute(new ChangeStepCommandContext
            {
                Step = _currentViewModel.CurrentStepIndex + 1,
                Completed = ctx =>
                {
                   // if(_currentViewModel.CurrentStep = 
                }
            });
        }

        //private void OnNextClick(Object sender, RoutedEventArgs e)
        //{
        //    txtDomainController.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        //    txtAdministrator.GetBindingExpression(TextBox.TextProperty).UpdateSource();

        //    if (Validation.GetHasError(txtDomainController) == false &&
        //        Validation.GetHasError(txtAdministrator) == false)
        //    {
        //        _viewModel.ChangeStepCommand.Execute(new ChangeStepCommandContext
        //        {
        //            Step = 1,
        //            Completed = ctx =>
        //            {
        //                UpdateButtonsVisibility();
                      

        //            }
        //        });
        //    }
        //}

        private void OnBackClick(Object sender, RoutedEventArgs e)
        {
            _currentViewModel.ChangeStepCommand.Execute(new ChangeStepCommandContext
            {
                Step = _currentViewModel.CurrentStepIndex - 1,
                Completed = ctx =>
                {
                    
                }
            });
        
        }

        private void OnActiveDirectoryClick(object sender, RoutedEventArgs e)
        {
            SetCurrentViewModel(_scanDomainViewModel);
            UpdateViewVisibility();
        }

        private void OnWorkgroupClick(object sender, RoutedEventArgs e)
        {
            SetCurrentViewModel(_scanWorkgroupViewModel);
            UpdateViewVisibility();
        }

        private void OnIpClick(object sender, RoutedEventArgs e)
        {
            SetCurrentViewModel(_scanByIpViewModel);
            UpdateViewVisibility();
        }


        private void SetCurrentViewModel(WizardViewModel viewModel)
        {
            DataContext = viewModel;

            if (_currentViewModel != null)
            {
                _currentViewModel.ViewUnloadedCommand.Execute(null);
                _currentViewModel.Completed -= OnWizardCompleted;
                _currentViewModel.Cancelled -= OnWizardCancelled;
            }
            _currentViewModel = viewModel;
            if (_currentViewModel != null)
            {
                wizardBorder.Visibility = Visibility.Visible;
                pathSelector.Visibility = Visibility.Collapsed;
                _currentViewModel.ViewLoadedCommand.Execute(null);
                _currentViewModel.Completed += OnWizardCompleted;
                _currentViewModel.Cancelled += OnWizardCancelled;
            }
        }

        private void OnWizardCancelled(Object sender, EventArgs e)
        {
            SetCurrentViewModel(null);
            wizardBorder.Visibility = Visibility.Collapsed;
            pathSelector.Visibility = Visibility.Visible;
            ToolTipIcon.Visibility = Visibility.Collapsed;
        }

        private void OnWizardCompleted(Object sender, EventArgs e)
        {
            if (FinishClick != null)
            {
                FinishClick(this, EventArgs.Empty);
            }
        }

        private void OnUnloaded(Object sender, RoutedEventArgs e)
        {
            if (_currentViewModel != null)
            {
                _currentViewModel.ViewUnloadedCommand.Execute(null);
            }
        }

        private void UpdateViewVisibility()
        {
            viewScanByIp.Visibility = Visibility.Collapsed;
            viewScanDomain.Visibility = Visibility.Collapsed;
            viewScanWorkgroup.Visibility = Visibility.Collapsed;
            ToolTipIcon.Visibility = Visibility.Collapsed;

            if(_currentViewModel == null)
            {
                return;
            }

            if (_currentViewModel is StartScanDomainViewModel)
            {
                viewScanDomain.Visibility = Visibility.Visible;
            }
            else if (_currentViewModel is StartScanWorkgroupViewModel)
            {
                viewScanWorkgroup.Visibility = Visibility.Visible;    
            }
            else if (_currentViewModel is StartScanByIpViewModel)
            {
                ToolTipIcon.Visibility = Visibility.Visible;
                viewScanByIp.Visibility = Visibility.Visible;
    
                string toolTipText = @"Note: The following templates of IP addresses or hostnames are allowed (for example).
IP             : 10.0.0.1
IP Range  : 10.0.0.1-10.0.0.5
IP Subnet : 10.0.0.0/24 -or- 10.0.0.0/255.255.255.0
Computer Name: HomeComputer";

                //ToolTipService.SetToolTip(ToolTipIcon, toolTipText);

                tooltip = new ToolTip { Content = toolTipText };
                tooltip.IsOpen = true;
                tooltip.PlacementTarget = ToolTipIcon;
                tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;

                ToolTipIcon.ToolTip = tooltip;
            }
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (tooltip != null)
                tooltip.IsOpen = false;

        }
    }
}
