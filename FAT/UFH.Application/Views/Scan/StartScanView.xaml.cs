using System;
using System.Windows;
using System.Windows.Controls;
using UFH.Application.Models;
using UFH.Application.Models.Directory;
using APTAT.Common;
using UFH.Application.Models.ModelBase;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace UFH.Application
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

            viewScanLocal.QuickScanAction = OnQuickScanClick;
            viewScanLocal.FullScanAction = OnFinishClick;
            viewScanLocal.CustomScanAction = OnNextClick;

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

                _currentViewModel.ParentViewModel.CurrentStep = _currentViewModel.Step;
            }

            Dispatcher.BeginInvoke((Action)delegate { _tooltip.IsOpen = false; });
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
                _currentViewModel.ViewUnloadedCommand.Execute(true);
                _currentViewModel.ChangeStepCommand.Execute(new ChangeStepCommandContext
                {
                    Step = 0,

                });
                


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


        private ToolTip _tooltip = new ToolTip();
        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var button = sender as Button;

            if (button.GetValue(ToolTipHelper.ToolTipProperty) == null)
                button.SetValue(ToolTipHelper.ToolTipProperty, _tooltip);
        }

        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var button = sender as Button;

            button.SetValue(ToolTipHelper.ToolTipProperty, null);
        }

        public class ToolTipHelper : DependencyObject
        {
            public static readonly DependencyProperty ToolTipProperty =
                DependencyProperty.RegisterAttached("ToolTip", typeof(ToolTip),
                typeof(ToolTipHelper),
                new UIPropertyMetadata(null, OnToolTipChanged));

            public static ToolTip GetToolTip(DependencyObject obj)
            {
                return (ToolTip)obj.GetValue(ToolTipProperty);
            }

            public static void SetToolTip(DependencyObject obj, ToolTip value)
            {
                obj.SetValue(ToolTipProperty, value);
            }

            private static void OnToolTipChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
            {
                if (e.OldValue != null)
                {
                    (e.OldValue as ToolTip).Content = null;
                    (e.OldValue as ToolTip).IsOpen = false;
                }

                if (e.NewValue != null && obj is ContentControl)
                    ApplyToolTip(obj as ContentControl, e.NewValue as ToolTip);
            }

            private static void ApplyToolTip(ContentControl targetElement, ToolTip tooltip)
            {
                if (targetElement == null) return;

                Task.Delay(500).ContinueWith(_ => targetElement.Dispatcher.BeginInvoke((Action)delegate
                {
                    if (GetToolTip(targetElement) != null)
                    {
                        tooltip.PlacementTarget = targetElement;
                        tooltip.Content = targetElement.Content;
                        tooltip.Placement = PlacementMode.Mouse;
                        tooltip.IsOpen = true;
                    }
                }, DispatcherPriority.Normal));
            }
        }
    }
}
