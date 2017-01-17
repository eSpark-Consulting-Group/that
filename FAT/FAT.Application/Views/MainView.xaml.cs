using System;
using System.Windows;
using System.Windows.Navigation;
using FAT.Application.Models;
using FAT.Application.Services;
using Telerik.Windows;
using System.Diagnostics;
using System.Linq;
using APTAT.Core;
using APTAT.Valkyrie;
using Process = System.Diagnostics.Process;
using ScanCompletedEventArgs = FAT.Application.Services.ScanCompletedEventArgs;
using System.ComponentModel;
using FAT.Application.Messages;
using System.Threading.Tasks;
using FAT.Application.Models.Valkyrie;
using Telerik.Windows.Controls;
using System.Windows.Controls;

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
            _viewModel.ValkyrieAnalyzeStarted += ViewModelOnValkyrieAnalyzeStarted;
            _viewModel.ValkyrieAnalyzeCompleted += ViewModelOnValkyrieAnalyzeCompleted;
        }

        [MessageReceiver]
        public void AcceptMessage(ChangeScreen message)
        {
            switch (message.Message)
            {
                case ChangeScreenTypes.StartScan:
                    StartScanWindow details = new StartScanWindow();
                    details.Owner = System.Windows.Application.Current.MainWindow;
                    if (details.ShowDialog() == true && message.Action != null)
                    {
                        empty.Visibility = Visibility.Collapsed;

                        if (mainContent.Content == null)
                            mainContent.Content = new ResultTabsControl();
                        
                        message.Action(details.IsQuickScan);
                    }
                    break;
                case ChangeScreenTypes.StopScan:
                    _confirmation = new ConfirmationWindow();
                    _confirmation.Owner = System.Windows.Application.Current.MainWindow;
                    if (_confirmation.ShowDialog() == true && message.Action != null)
                        message.Action(null);
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

        private void OnScanCompleted(Object sender, ScanCompletedEventArgs e)
        {
            if (_onCompletedAction != null)
            {
                _onCompletedAction();
                return;
            }

            if (_confirmation != null && _confirmation.DialogResult == null)
                _confirmation.DialogResult = false;

            ScanCompletedWindow scanCompletedWindow = new ScanCompletedWindow();
            scanCompletedWindow.Text.Text = String.Empty;
            scanCompletedWindow.Text.Text += "Scan Summary:";
            scanCompletedWindow.Text.Text += String.Format("{0}{1} Total Scan Attempts", Environment.NewLine, e.SuccededCount + e.CanceledCount + e.FailedCount + e.NotPerformedCount);
            scanCompletedWindow.Text.Text += Environment.NewLine;

            if (e.SuccededCount > 0) scanCompletedWindow.Text.Text += String.Format("- {0} Successful Scans", e.SuccededCount);
            if (e.CanceledCount > 0) scanCompletedWindow.Text.Text += String.Format("{0}- {1} Canceled Scans", Environment.NewLine, e.CanceledCount);
            if (e.FailedCount > 0) scanCompletedWindow.Text.Text += String.Format("{0}- {1} Failed Scans", Environment.NewLine, e.FailedCount);
            if (e.NotPerformedCount > 0) scanCompletedWindow.Text.Text += String.Format("{0}- {1} Computers Inaccessible", Environment.NewLine, e.NotPerformedCount);

            scanCompletedWindow.Text.Text += Environment.NewLine + Environment.NewLine + "Threat Detection Summary:";
            int total = e.FilesStatistics.UnknownFilesCount + e.FilesStatistics.MalwareFilesCount + e.FilesStatistics.FailedToAnalyzeFilesCount + e.FilesStatistics.NoVerdictFilesCount;
            if (e.FilesStatistics.TotalFilesCount > 0)
            {
                scanCompletedWindow.Text.Text += String.Format("{0}{1} Total Threats Found*", Environment.NewLine, total);
            }
            scanCompletedWindow.Text.Text += String.Format("{0}- {1} Unknown Files", Environment.NewLine, e.FilesStatistics.UnknownFilesCount);

            if (e.FilesStatistics.MalwareFilesCount > 0) scanCompletedWindow.Text.Text += String.Format("{0}- {1} Malicious Files", Environment.NewLine, e.FilesStatistics.MalwareFilesCount);
            if (e.FilesStatistics.FailedToAnalyzeFilesCount > 0 || e.FilesStatistics.NoVerdictFilesCount > 0) scanCompletedWindow.Text.Text += String.Format("{0}- {1} Lookup in Progress", Environment.NewLine, e.FilesStatistics.FailedToAnalyzeFilesCount + e.FilesStatistics.NoVerdictFilesCount);

            scanCompletedWindow.Text.Text += String.Format("{0}{1}* {2} files have unique hash values", Environment.NewLine, Environment.NewLine, e.DistinctFileFound);

            if (e.FilesStatistics.MalwareFilesCount > 0 || e.FilesStatistics.UnknownFilesCount > 0 || e.FilesStatistics.FailedToAnalyzeFilesCount > 0 || e.FilesStatistics.NoVerdictFilesCount > 0)
                scanCompletedWindow.path2.Visibility = Visibility.Visible;
            else
                scanCompletedWindow.path1.Visibility = Visibility.Visible;

            scanCompletedWindow.Owner = System.Windows.Application.Current.MainWindow;
            if (e.SuccededCount > 0)
            {
                scanCompletedWindow.DataContext = DataContext;
                /*if (scanCompletedWindow.ShowDialog() == true)
                    ShowRegisterToValkyrieWindow();
                else*/
                if (e.FilesStatistics.UnknownFilesCount > 0
             || e.FilesStatistics.FailedToAnalyzeFilesCount > 0
             || e.FilesStatistics.NoVerdictFilesCount > 0)
                {
                    _viewModel.IsAnalyseWithValkyrieEnabled = true;
                    _viewModel.BeginValkyrieAnalysis();
                }
                else
                    scanCompletedWindow.ShowDialog();

                if (e.FilesStatistics.TotalFilesCount != e.FilesStatistics.SafeFilesCount)
                    _viewModel.GroupingEnabled = true;
            }

        }

        private CamCredentialsWindow _dialog;
        private void ShowRegisterToValkyrieWindow()
        {
            _dialog = new CamCredentialsWindow();
            _dialog.Owner = System.Windows.Application.Current.MainWindow;
            _dialog.OkClick += (sender, args) =>
            {
                if (_viewModel.BeginValkyrieAnalysis())
                    Dispatcher.Invoke((Action)delegate
                    {
                        _dialog.loading.IsBusy = true;
                        _dialog.ok.Visibility = Visibility.Collapsed;
                    });
            };
            _dialog.CancelClick += (sender, args) =>
            {
                _viewModel.IsAnalyseWithValkyrieEnabled = true;
            };

            _dialog.ShowDialog();
        }

        private void ViewModelOnValkyrieAnalyzeStarted(object sender, EventArgs eventArgs)
        {
            if (_dialog == null) return;
            Dispatcher.Invoke((Action)delegate
            {
                _dialog.Close();
                _dialog = null;
            });
        }

        private void ViewModelOnValkyrieAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<ValkyrieFile> e)
        {
            if (_onCompletedAction != null)
            {
                _onCompletedAction();
                return;
            }
            if (_dialog == null) return;
            if (e.Error == null) return;
            var error = e.Error as AggregateException;
            var exception = error != null ? error.InnerExceptions.First() : e.Error;
            if (exception is InvalidOperationException || exception is ValkyrieRegistrationFailedException)
                _dialog.SetErrorMessage(e.Error.Message);
            Dispatcher.Invoke((Action)delegate
            {
                _dialog.ok.Visibility = Visibility.Visible;
                _dialog.loading.IsBusy = false;
            });
        }

        private void ValkyrieClick(Object sender, RoutedEventArgs e)
        {
            ShowRegisterToValkyrieWindow();

        }

        private static void ShowReport(RadMenuItem menuItem)
        {
            ReportWindow details = new ReportWindow();
            details.Header = menuItem.Tag as string;
            details.Owner = System.Windows.Application.Current.MainWindow;
            details.ShowDialog();
        }

        private void OnExecutiveReportClick(Object sender, RadRoutedEventArgs e)
        {
            _viewModel.SetReportKind(ReportKind.Executive);
            ReportButton.IsOpen = false;
            ShowReport(sender as RadMenuItem);
        }

        private void OnPerDeviceReportClick(Object sender, RadRoutedEventArgs e)
        {
            _viewModel.SetReportKind(ReportKind.DetailedPerDevice);
            ReportButton.IsOpen = false;
            ShowReport(sender as RadMenuItem);
        }

        private void OnPerProgramReportClick(Object sender, RadRoutedEventArgs e)
        {
            _viewModel.SetReportKind(ReportKind.DetailedPerProgram);
            ReportButton.IsOpen = false;
            ShowReport(sender as RadMenuItem);
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
            loading.BusyContent = "Closing...";
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

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as Button).Command.Execute(null);
        }
    }
}
