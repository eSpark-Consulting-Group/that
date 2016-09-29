using System;
using System.Windows;
using System.Windows.Controls;
using APTAT.Application.Models;
using APTAT.Application.Models.Directory;
using APTAT.Common;

namespace APTAT.Application
{


    public class StartScanEventArgs : EventArgs
    {
        public bool IsQuickScan { get; set; }
    }
    /// <summary>
    /// Interaction logic for StartScanView.xaml
    /// </summary>
    public partial class StartScanView
    {
        public event EventHandler CancelClick;
        public event EventHandler<StartScanEventArgs> FinishClick;

        private readonly WizardViewModel _scanDomainViewModel;
        private readonly WizardViewModel _scanWorkgroupViewModel;
        private readonly WizardViewModel _scanByIpViewModel;
        private readonly WizardViewModel _scanLocalViewModel;

        private WizardViewModel _currentViewModel;


        private bool _isQuickScan;

        public StartScanView()
        {
            InitializeComponent();

            _scanDomainViewModel = CompositionHelper.Instance.Resolve<StartScanDomainViewModel>();
            _scanWorkgroupViewModel = CompositionHelper.Instance.Resolve<StartScanWorkgroupViewModel>();
            _scanByIpViewModel = CompositionHelper.Instance.Resolve<StartScanByIpViewModel>();
            _scanLocalViewModel = CompositionHelper.Instance.Resolve<StartScanLocalViewModel>();

            viewScanByIp.DataContext = _scanByIpViewModel;
            viewScanDomain.DataContext = _scanDomainViewModel;
            viewScanWorkgroup.DataContext = _scanWorkgroupViewModel;
            viewScanLocal.DataContext = _scanLocalViewModel;

            Unloaded += OnUnloaded;
        }

        private void OnCancelClick(Object sender, RoutedEventArgs e)
        {
            if (CancelClick != null)
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
                IsNext =  true,
                Completed = ctx =>
                {
                }
            });
        }

        private void OnBackClick(Object sender, RoutedEventArgs e)
        {




            _currentViewModel.ChangeStepCommand.Execute(new ChangeStepCommandContext
            {
                Step = _currentViewModel.CurrentStepIndex - 1,

            });

            if (_currentViewModel == null)
                cancel.Visibility = Visibility.Visible;


        }


        private void OnActiveDirectoryClick(object sender, RoutedEventArgs e)
        {
            
            SetCurrentViewModel(_scanDomainViewModel);
            var parent = (MainViewViewModel)_currentViewModel.ParentViewModel;
            parent.UseCurrentCredentials = true;
            parent.UseCurrentDomainForDeploy = false;
            UpdateViewVisibility();
        }

        private void OnWorkgroupClick(object sender, RoutedEventArgs e)
        {
            SetCurrentViewModel(_scanWorkgroupViewModel);
            var parent = (MainViewViewModel)_currentViewModel.ParentViewModel;
            parent.UseCurrentCredentials = true;
            parent.UseCurrentDomainForDeploy = false;
            UpdateViewVisibility();
        }

        private void OnIpClick(object sender, RoutedEventArgs e)
        {
            SetCurrentViewModel(_scanByIpViewModel);
            UpdateViewVisibility();
        }

        private void OnThisClick(object sender, RoutedEventArgs e)
        {
            SetCurrentViewModel(_scanLocalViewModel);
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

        }

        private void OnWizardCompleted(Object sender, EventArgs e)
        {
            if (FinishClick != null)
            {
                var arg = new StartScanEventArgs() { IsQuickScan = _isQuickScan };
                FinishClick(this, arg);
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
            viewScanLocal.Visibility = Visibility.Collapsed;
            cancel.Visibility = Visibility.Visible;

            if (_currentViewModel == null)
            {
                return;
            }

            cancel.Visibility = Visibility.Collapsed;
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
                viewScanByIp.Visibility = Visibility.Visible;
            }
            else if (_currentViewModel is StartScanLocalViewModel)
            {
                viewScanLocal.Visibility = Visibility.Visible;
            }
        }

        private void OnQuickScanClick(object sender, RoutedEventArgs e)
        {
            _isQuickScan = true;
            _currentViewModel.ChangeStepCommand.Execute(new ChangeStepCommandContext
            {

                Step = _currentViewModel.CurrentStepIndex + 1,
                Completed = ctx =>
                {
                }
            });
        }
    }
}
