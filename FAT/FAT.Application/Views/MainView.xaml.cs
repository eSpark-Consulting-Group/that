using System;
using System.Windows;
using System.Windows.Navigation;
using FAT.Application.Models;
using FAT.Application.Services;
using Telerik.Windows;
using System.Diagnostics;
using System.Linq;
using APTAT.Valkyrie;
using System.ComponentModel;
using APTAT.Application.Services.EventArgs;
using FAT.Application.Messages;
using Telerik.Windows.Controls;
using FAT.Application.Views;
using static FAT.Application.Messages.AppMessages;
using FAT.Application.Extensions;

namespace FAT.Application
{
    public partial class MainView
    {
        private MainViewViewModel _viewModel;

        public MainView()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                MessageManager.Instance.RegisterListener(this);
            }

            DataContext = App.Current.MainWindow.DataContext;
            _viewModel = DataContext as MainViewViewModel;

            _viewModel.ScanProcessCompleted += OnScanCompleted;
            _viewModel.ErrorOccurred += OnErrorOccurred;
        }

        [MessageReceiver]
        public void AcceptMessage(AppMessage<object> message)
        {
            switch (message.Type)
            {
                case MessageTypes.StartScan:
                    StartScanWindow details = new StartScanWindow();
                    details.Owner = App.Current.MainWindow;
                    if (details.ShowDialog() == true && message.Action != null)
                        message.Action(details.IsQuickScan);
                    break;
                case MessageTypes.StopScan:
                    _confirmation = new ConfirmationWindow();
                    _confirmation.Owner = App.Current.MainWindow;
                    if (_confirmation.ShowDialog() == true && message.Action != null)
                        message.Action(null);
                    break;
            }
        }

        [MessageReceiver]
        public void AcceptMessage(AppMessage<SearchUserControl> message)
        {
            switch (message.Type)
            {
                case MessageTypes.FilterTarget:
                    message.Action(_viewModel.CurrentScanAvailable ? SearchTextBox2 : SearchTextBox);
                    break;
            }
        }

        private ConfirmationWindow _confirmation;

        private void OnErrorOccurred(Object sender, DomainEventArgs e)
        {
            ErrorWindow error = new ErrorWindow
            {
                Text = {Text = e.Exception.Message},
                Owner = App.Current.MainWindow
            };
            error.ShowDialog();
        }

        private void OnScanCompleted(Object sender, EventArgs e)
        {
            if (_onCompletedAction != null)
            {
                _onCompletedAction();
                return;
            }

            if (_confirmation != null && _confirmation.DialogResult == null)
                _confirmation.DialogResult = false;

            if (!_viewModel.IsScanCancelled && !_viewModel.IsScanFailed)
            {
                var result = new ScanResultWindow();
                result.DataContext = _viewModel;
                result.Owner = App.Current.MainWindow;
                result.GetVisual = () => SearchTextBox;
                result.ShowDialog();
            }
        }

        private void ViewModelOnValkyrieAnalyzeCompleted(object sender, ErrorEventArgs e)
        {
            if (_onCompletedAction != null)
            {
                _onCompletedAction();
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

        public bool IsDiscoveryInProgress
        {
            get
            {
                return _viewModel.IsAutoDiscoveryInProgress || _viewModel.IsScanInProgress == null;
            }
        }

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

            SettingsWindow options = new SettingsWindow { Owner = System.Windows.Application.Current.MainWindow, DataContext = DataContext};
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