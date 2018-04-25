using System;
using System.Windows;
using System.Windows.Navigation;
using UFH.Application.Models;
using UFH.Application.Services;
using Telerik.Windows;
using System.Diagnostics;
using System.Linq;
using APTAT.Core.Valkyrie;
using System.ComponentModel;
using APTAT.Application.Services.EventArgs;
using UFH.Application.Messages;
using Telerik.Windows.Controls;
using UFH.Application.Views;
using static UFH.Application.Messages.AppMessages;
using UFH.Application.Extensions;
using APTAT.Application;
using System.Net;
using APTAT.Common;
using APTAT.Application.Models;
using System.Threading.Tasks;
using System.Windows.Input;
using APTAT.COT.Login;

namespace UFH.Application
{
    public partial class MainView
    {
        private MainViewModel _viewModel;
        private SecurityUserCredentials Credentials { get; set; }

        public MainView()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                MessageManager.Instance.RegisterListener(this);
            }

            DataContext = App.Current.MainWindow.DataContext;
            _viewModel = DataContext as MainViewModel;

            _viewModel.ScanProcessCompleted += OnScanCompleted;
            _viewModel.ErrorOccurred += OnErrorOccurred;
        }

        private void LogOnClick(object sender, RoutedEventArgs e)
        {
            //_viewModel.LogInTxt = "LogOn";
            _viewModel.LogedIn = !_viewModel.LogedIn;
            LogOnButton.IsOpen = !_viewModel.LogedIn;
            //LogonText.Foreground = 
            ShowRegisterToValkyrieWindow();
        }

        private void LogoutMenuItemClick(object sender, RoutedEventArgs e)
        {
            //TODO:
        }

        public string LoggedUserName
        {
            get { return _viewModel.LoggedUserName; }
        }

        private void ShowRegisterToValkyrieWindow(bool isScanCompleted = false)
        {
            //if (!isScanCompleted)
            {
                var _dialog = new CamCredentialsWindow
                {
                    Owner = System.Windows.Application.Current.MainWindow,
                };
                _dialog.OkClick += (sender, args) =>
                {
                    if (_dialog.DataContext is ValkyrieRegistrationModel m)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            _dialog.loading.IsBusy = true;
                            _dialog.ok.Visibility = Visibility.Collapsed;
                        });

                        Credentials = new SecurityUserCredentials(m.UserName, m.Password, m.LicenseId);
                        if (Credentials.CanApplay)
                        {
                            Task.Run(() => { m.LoginResult = LoginHelper.Login(Credentials); }).Wait();
                        }

                        Dispatcher.Invoke(() =>
                        {
                            _dialog.ok.Visibility = Visibility.Visible;
                            _dialog.loading.IsBusy = false;
                        });

                        _dialog.SetErrorMessage(m.LoginResult.ToString());
                        if (m.LoginResult == LoginResults.SUCCESS_AUTHENTICATION)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _dialog.Close();
                                _dialog = null;
                            });
                        }
                    }
                };
                _dialog.CancelClick += (sender, args) =>
                {
                    //LogonText.Text = string.Empty;
                };

                _dialog.OnClosingEvent += (sender, arg) =>
                {
                    //LogonText.Text = string.Empty;
                    if (_dialog.DataContext is ValkyrieRegistrationModel m)
                    {
                        if (m.LoginResult == LoginResults.SUCCESS_AUTHENTICATION)
                        {
                            if (Credentials.CanApplay)
                            {
                                _viewModel.LogInTxt = Credentials.Email.Split('@').FirstOrDefault();
                            }
                        }
                    }

                    if (IsScanCompleted && _viewModel.BeginValkyrieAnalysis())
                    {
                        Dispatcher.Invoke(() =>
                        {
                            _dialog.loading.IsBusy = true;
                            _dialog.ok.Visibility = Visibility.Collapsed;
                        });
                    }
                };

                _dialog.OnGuestButtonClick += (sender, arg) =>
                 {
                     LoginHelper.Logout();
                     Credentials = new SecurityUserCredentials();
                     _viewModel.LogInTxt = "Guest";
                     Dispatcher.Invoke(() =>
                     {
                         _dialog.Close();
                         _dialog = null;
                     });
                 };

                if (isScanCompleted && Credentials.CanApplay) return;
                _dialog.ShowDialog();
            }
        }

        [MessageReceiver]
        public void AcceptMessage(AppMessage<object> message)
        {
            switch (message.Type)
            {
                case MessageTypes.StartScan:
                    IsScanCompleted = false;
                    StartScanWindow details = new StartScanWindow();
                    details.Owner = App.Current.MainWindow;
                    if (details.ShowDialog() == true && message.Action != null)
                        message.Action(details.IsQuickScan);
                    break;
                case MessageTypes.StopScan:
                    IsScanCompleted = true;
                    _confirmation = new ConfirmationWindow();
                    _confirmation.Owner = App.Current.MainWindow;
                    if (_confirmation.ShowDialog() == true && message.Action != null)
                        message.Action(null);
                    //ShowReport(new RadMenuItem { Tag = "Executive Report" });
                    break;
            }
        }

        [MessageReceiver]
        public void AcceptMessage(AppMessage<SearchUserControl> message)
        {
            message.Action(_viewModel.CurrentScanAvailable ? SearchTextBox2 : SearchTextBox);
        }

        private ConfirmationWindow _confirmation;

        private void OnErrorOccurred(Object sender, DomainEventArgs e)
        {
            ErrorWindow error = new ErrorWindow
            {
                Text = { Text = e.Exception.Message },
                Owner = App.Current.MainWindow
            };
            error.ShowDialog();
        }

        private void OnScanCompleted(Object sender, EventArgs e)
        {
            IsScanCompleted = true;
            if (_onCompletedAction != null)
            {
                _viewModel.OnAppClose(_onCompletedAction);
                return;
            }

            if (_confirmation != null && _confirmation.DialogResult == null)
                _confirmation.DialogResult = false;

            if (_viewModel.ShowResult)
            {
                var result = new ScanResultWindow
                {
                    DataContext = _viewModel,
                    Owner = App.Current.MainWindow,
                    GetVisual = () => searchGrid
                };
                result.ShowDialog();
            }

            ShowRegisterToValkyrieWindow(true);
        }

        private void ViewModelOnValkyrieAnalyzeCompleted(object sender, ErrorEventArgs e)
        {
            if (_onCompletedAction != null)
            {
                _viewModel.OnAppClose(_onCompletedAction);
                return;
            }

            if (e.Error == null) return;
            var error = e.Error as AggregateException;
            var exception = error != null ? error.InnerExceptions.First() : e.Error;
            if (exception is InvalidOperationException || exception is ValkyrieRegistrationFailedException)
            {
                var err = new ErrorWindow();
                err.Text.Text = e.Error.Message;
                err.Owner = App.Current.MainWindow;
                err.ShowDialog();
            }
        }


        private static void ShowReport(RadMenuItem menuItem)
        {
            ReportWindow details = new ReportWindow();
            details.Header = menuItem.Tag as string;
            details.Owner = App.Current.MainWindow;
            details.ShowDialog();
        }

        private void OnExecutiveReportValkyrieClick(Object sender, RadRoutedEventArgs e)
        {
            _viewModel.SetReportKind(ReportKind.ExecutiveWithValkyrie);
            ReportButton.IsOpen = false;
            ShowReport(sender as RadMenuItem);
        }

        private void OnPerDeviceReportValkyrieClick(Object sender, RadRoutedEventArgs e)
        {
            _viewModel.SetReportKind(ReportKind.DetailedPerDeviceWithValkyrie);
            ReportButton.IsOpen = false;
            ShowReport(sender as RadMenuItem);
        }

        private void OnPerProgramReportValkyrieClick(Object sender, RadRoutedEventArgs e)
        {
            _viewModel.SetReportKind(ReportKind.DetailedPerProgramWithValkyrie);
            ReportButton.IsOpen = false;
            ShowReport(sender as RadMenuItem);
        }

        public bool IsScanInProgress
        {
            get
            {
                return _viewModel.IsScanInProgress.GetValueOrDefault(false);
            }
        }

        //public bool IsLogOnPossible
        //{
        //    get
        //    {
        //        return !_viewModel.IsAnyScanInProgress;
        //    }
        //}

        public bool IsDiscoveryInProgress
        {
            get
            {
                return _viewModel.IsAutoDiscoveryInProgress || _viewModel.IsScanInProgress == null;
            }
        }

        public bool IsScanCompleted { get; private set; }

        private Action _onCompletedAction;
        public void OnAppClose(Action action)
        {
            _onCompletedAction = action;
            BusyContent.Text = "Closing...";
            _viewModel.OnAppClose(action);
        }

        private void OptionsClick(object sender, RoutedEventArgs e)
        {
            int tcpShort, udpShort;
            int.TryParse(this._viewModel.TcpPort, out tcpShort);
            int.TryParse(this._viewModel.UdpPort, out udpShort);

            SettingsWindow options = new SettingsWindow { Owner = System.Windows.Application.Current.MainWindow, DataContext = DataContext };
            if (options.ShowDialog() == true)
            {
                short tcpShortNew;
                short.TryParse(this._viewModel.TcpPort, out tcpShortNew);
                short udpShortNew;
                short.TryParse(this._viewModel.UdpPort, out udpShortNew);
                if (tcpShortNew != tcpShort || udpShortNew != udpShort)
                {
                    _viewModel.OnPortChanged(udpShortNew, tcpShortNew);
                }
            }
        }

        private void AboutClick(Object sender, RoutedEventArgs e)
        {
            HelpButton.IsOpen = false;
            AboutWindow about = new AboutWindow { Owner = App.Current.MainWindow };
            about.ShowDialog();
        }

        private void AgentClick(Object sender, RoutedEventArgs e)
        {
            HelpButton.IsOpen = false;
            AgentWindow agent = new AgentWindow { Owner = App.Current.MainWindow };
            agent.ShowDialog();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(_viewModel.ValkyrieDetailsLink));
            e.Handled = true;
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(_viewModel.PreviousScansViewModel.ValkyrieDetailsLink));
            e.Handled = true;
        }

        private void EditEmail(object sender, RoutedEventArgs e)
        {
            email.DelayedFocus();
        }
    }
}