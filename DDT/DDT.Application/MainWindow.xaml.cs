using System.Windows;
using DDT.Application.Transitions;
using DDT.Application.Extensions;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.ComponentModel;
using DDT.Application.Messages;
using DDT.Application.ViewModels;
using System.Windows.Controls;
using System.Windows.Data;
using System;
using APTAT.Common;
using DDT.Application.Views;

namespace DDT.Application
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = CompositionHelper.Instance.Resolve<MainViewModel>();

                BindingOperations.SetBinding(this, ErrorMessageProperty, new Binding("ErrorMessage"));
                BindingOperations.SetBinding(this, DisclaimerMessageProperty, new Binding("DisclaimerMessage"));
                BindingOperations.SetBinding(this, OnLoadedCommandProperty, new Binding("OnLoadedCommand"));

                TransitionManager.Presenter = transitionPresenter;
                TransitionManager.CurrentStrategy.CurrentTarget = boxEULA;

                MessageManager.Instance.RegisterListener(this);

                Loaded += MainWindow_Loaded;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            OnLoadedCommand?.Execute(null);
        }

        #region WizardHeaderProperty (DependencyProperty)

        public string WizardHeader
        {
            get { return (string)GetValue(WizardHeaderProperty); }
            set { SetValue(WizardHeaderProperty, value); }
        }

        public static readonly DependencyProperty WizardHeaderProperty =
            DependencyProperty.Register("WizardHeader", typeof(string), typeof(MainWindow),
            new PropertyMetadata(string.Empty, OnWizardHeaderChanged));

        private static void OnWizardHeaderChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as MainWindow;
            var value = e.NewValue as string;
            var oldValue = e.OldValue as string;

            if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(oldValue))
            {
                if (TransitionManager.CurrentStrategy is NoneTransitionStrategy)
                    VisualStateManager.GoToElementState(control.grid1, "Up2", false);
                else
                    VisualStateManager.GoToElementState(control.grid1, "Up", false);
            }

            if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(oldValue))
            {
                if (TransitionManager.CurrentStrategy is NoneTransitionStrategy)
                    VisualStateManager.GoToElementState(control.grid1, "Down2", false);
                else
                    VisualStateManager.GoToElementState(control.grid1, "Down", false);
            }
        }

        #endregion

        #region ErrorMessageProperty (DependencyProperty)

        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(MainWindow),
            new PropertyMetadata(string.Empty, OnErrorMessageChanged));

        private static void OnErrorMessageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as MainWindow;
            var value = e.NewValue as string;
            var oldValue = e.OldValue as string;

            if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(oldValue))
            {
                if (TransitionManager.CurrentStrategy is NoneTransitionStrategy)
                    VisualStateManager.GoToElementState(control.grid2, "Error2", false);
                else
                    VisualStateManager.GoToElementState(control.grid2, "Error", false);
            }

            if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(oldValue))
            {
                if (TransitionManager.CurrentStrategy is NoneTransitionStrategy)
                    VisualStateManager.GoToElementState(control.grid2, "NoError2", false);
                else
                    VisualStateManager.GoToElementState(control.grid2, "NoError", false);
            }
        }

        #endregion

        #region DisclaimerMessageProperty (DependencyProperty)

        public string DisclaimerMessage
        {
            get { return (string)GetValue(DisclaimerMessageProperty); }
            set { SetValue(DisclaimerMessageProperty, value); }
        }

        public static readonly DependencyProperty DisclaimerMessageProperty =
            DependencyProperty.Register("DisclaimerMessage", typeof(string), typeof(MainWindow),
            new PropertyMetadata(string.Empty, OnDisclaimerMessageChanged));

        private static void OnDisclaimerMessageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as MainWindow;
            var value = e.NewValue as string;
            var oldValue = e.OldValue as string;

            if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(oldValue))
            {
                if (TransitionManager.CurrentStrategy is NoneTransitionStrategy)
                    VisualStateManager.GoToElementState(control.grid3, "Disclaimer2", false);
                else
                    VisualStateManager.GoToElementState(control.grid3, "Disclaimer", false);
            }

            if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(oldValue))
            {
                if (TransitionManager.CurrentStrategy is NoneTransitionStrategy)
                    VisualStateManager.GoToElementState(control.grid3, "NoDisclaimer2", false);
                else
                    VisualStateManager.GoToElementState(control.grid3, "NoDisclaimer", false);
            }
        }

        #endregion

        #region OnLoadedCommandProperty (DependencyProperty)

        public ICommand OnLoadedCommand
        {
            get { return (ICommand)GetValue(OnLoadedCommandProperty); }
            set { SetValue(OnLoadedCommandProperty, value); }
        }

        public static readonly DependencyProperty OnLoadedCommandProperty =
            DependencyProperty.Register("OnLoadedCommand", typeof(ICommand), typeof(MainWindow),
            new PropertyMetadata(null));

        #endregion

        [MessageReceiver]
        public void AcceptMessage(ChangeScreen message)
        {
            Welcome.Visibility = Visibility.Collapsed;
            HeaderMenuGrid.Visibility = Visibility.Visible;

            switch (message.Screen)
            {
                case ChangeScreenTypes.Packages:
                    if (ApplyTransition(boxPackages, StartButton, message.Direction))
                    {
                        message.Action(message.Screen);
                        WizardHeader = "Start Deployment (1/3)";
                    }
                    break;
                case ChangeScreenTypes.DiscoveryTargets:
                    if (ApplyTransition(boxDiscoveryTargets, StartButton, message.Direction))
                    {
                        message.Action(message.Screen);
                        WizardHeader = "Start Deployment (2/3)";
                    }
                    break;
                case ChangeScreenTypes.ActiveDirectoryCredentials:
                    if (ApplyTransition(boxActiveDirectoryCredentials, StartButton, message.Direction))
                    {
                        message.Action(message.Screen);
                        WizardHeader = "Discover Active Directory (1/2)";
                    }
                    break;
                case ChangeScreenTypes.ActiveDirectory:
                    if (ApplyTransition(boxActiveDirectory, StartButton, message.Direction))
                    {
                        message.Action(message.Screen);
                        WizardHeader = "Discover Active Directory (2/2)";
                    }
                    break;
                case ChangeScreenTypes.WorkgroupCredentials:
                    if (ApplyTransition(boxWorkgroupCredentials, StartButton, message.Direction))
                    {
                        message.Action(message.Screen);
                        WizardHeader = "Discover Workgroup (1/2)";
                    }
                    break;
                case ChangeScreenTypes.Workgroup:
                    if (ApplyTransition(boxWorkgroup, StartButton, message.Direction))
                    {
                        message.Action(message.Screen);
                        WizardHeader = "Discover Workgroup (2/2)";
                    }
                    break;
                case ChangeScreenTypes.NetworkAddresses:
                    if (ApplyTransition(boxNetworkAddresses, StartButton, message.Direction))
                    {
                        message.Action(message.Screen);
                        WizardHeader = "Discover Network Address";
                    }
                    break;
                case ChangeScreenTypes.DeploymentCredentials:
                    if (ApplyTransition(boxDeploymentCredentials, StartButton, message.Direction))
                    {
                        message.Action(message.Screen);
                        WizardHeader = "Start Deployment (3/3)";
                    }
                    break;
                case ChangeScreenTypes.DeploymentResults:
                    if (ApplyTransition(boxDeploymentResults, StopButton, message.Direction))
                    {
                        message.Action(message.Screen);
                        WizardHeader = null;
                    }
                    break;
                case ChangeScreenTypes.Report:
                    if (ApplyTransition(boxReport, StartButton, message.Direction))
                    {
                        message.Action(message.Screen);
                        WizardHeader = null;
                    }
                    break;
                case ChangeScreenTypes.Options:
                    if (ApplyTransition(boxOptions, OptionsButton))
                    {
                        message.Action(message.Screen);
                        WizardHeader = null;
                    }
                    break;
                case ChangeScreenTypes.About:
                    if (ApplyTransition(boxAbout, AboutButton))
                    {
                        message.Action(message.Screen);
                        WizardHeader = null;
                    }
                    break;
                case ChangeScreenTypes.Confirmation:
                    var confirmation = new ConfirmationWindow();
                    confirmation.Owner = App.Current.MainWindow;
                    confirmation.Resources = confirmation.Owner.Resources;
                    if (confirmation.ShowDialog() == true)
                        message.Action(ChangeScreenTypes.Confirmation);
                    break;
            }
        }

        private ToggleButton _previousButton;
        private bool ApplyTransition(FrameworkElement element, ToggleButton button, Direction? direction = null)
        {
            bool apply = TransitionManager.CurrentStrategy.ApplyTransition(element, direction);

            if (button == null)
                return apply;

            if (apply)
            {
                AboutButton.IsChecked = AboutButton.Name == element.Name;
                OptionsButton.IsChecked = OptionsButton.Name == element.Name;
                StartButton.IsChecked = StartButton.Name == element.Name;
                StopButton.IsChecked = StopButton.Name == element.Name;
                _previousButton = button;
            }
            else
                button.DelayedToggle(false);

            _previousButton.DelayedFocus();

            return apply;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e) => ((MainViewModel)DataContext).StopDeployCommand.Execute((Action)App.Current.Shutdown);

        private void OnResizeClick(object sender, RoutedEventArgs e) => SwitchState(sender as Button);

        private void OnMinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void SwitchState(Button sender)
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    {
                        WindowState = WindowState.Maximized;
                        var restoreStyle = FindResource("RestoreWindowButton") as Style;
                        sender.Style = restoreStyle;
                        sender.ToolTip = "Restore";
                        break;
                    }
                case WindowState.Maximized:
                    {
                        WindowState = WindowState.Normal;
                        var maxStyle = FindResource("MaximizeButton") as Style;
                        sender.Style = maxStyle;
                        sender.ToolTip = "Maximize";
                        break;
                    }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (WindowState == WindowState.Maximized) return;
            DragMove();
        }
    }
}