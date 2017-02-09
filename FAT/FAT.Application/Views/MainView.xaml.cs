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
            _viewModel.DomainTargetsResolveFailure += OnDomainTargetsResolveFailure;
        }

        [MessageReceiver]
        public void AcceptMessage(AppMessage<object> message)
        {
            switch (message.Type)
            {
                case MessageTypes.StartScan:
                    StartScanWindow details = new StartScanWindow();
                    details.Owner = System.Windows.Application.Current.MainWindow;
                    if (details.ShowDialog() == true && message.Action != null)
                    {
                        message.Action(details.IsQuickScan);
                    }
                    break;
                case MessageTypes.StopScan:
                    _confirmation = new ConfirmationWindow();
                    _confirmation.Owner = System.Windows.Application.Current.MainWindow;
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
                    message.Action(SearchTextBox);
                    break;
            }
        }

        private ConfirmationWindow _confirmation;

        private void OnDomainTargetsResolveFailure(Object sender, DomainEventArgs e)
        {
            ErrorWindow error = new ErrorWindow();
            error.Text.Text = e.Exception.Message;
            error.Owner = System.Windows.Application.Current.MainWindow;
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
                err.Owner = System.Windows.Application.Current.MainWindow;
                err.ShowDialog();
            }
        }


        private static void ShowReport(RadMenuItem menuItem)
        {
            ReportWindow details = new ReportWindow();
            details.Header = menuItem.Tag as string;
            details.Owner = System.Windows.Application.Current.MainWindow;
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
                return _viewModel.IsAnyScanInProgress;
            }
        }

        private Action _onCompletedAction;
        public void OnAppClose(Action action)
        {
            _onCompletedAction = action;
            BusyContent.Text = "Closing...";
            _viewModel.OnAppClose();
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
            AboutWindow about = new AboutWindow { Owner = System.Windows.Application.Current.MainWindow };
            about.ShowDialog();
        }

        private void AgentClick(Object sender, RoutedEventArgs e)
        {
            HelpButton.IsOpen = false;
            AgentWindow agent = new AgentWindow { Owner = System.Windows.Application.Current.MainWindow };
            agent.ShowDialog();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}