using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using APTAT.Application.Models;
using APTAT.Application.Services;
using Telerik.Windows;
using System.Diagnostics;
using System.Linq;
using APTAT.Core;
using APTAT.Core.Valkyrie;
using Process = System.Diagnostics.Process;
using ScanCompletedEventArgs = APTAT.Application.Services.ScanCompletedEventArgs;
using System.ComponentModel;
using APTAT.Application.Messages;
using APTAT.Application.Helpers;

namespace APTAT.Application
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

            DataContext = _viewModel = CompositionHelper.Instance.GetService<MainViewViewModel>();

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
                case ChangeScreenTypes.ValkyrieScan:
                    if (mainContent.Content != null && mainContent.Content is ResultTabsControl)
                        return;
                    mainContent.Content = new ResultTabsControl();
                    break;
                case ChangeScreenTypes.Scan:
                    if (mainContent.Content != null && mainContent.Content is ScanInfoTreeView)
                        return;
                    mainContent.Content = new ScanInfoTreeView();
                    break;
                case ChangeScreenTypes.StartScan:
                    StartScanWindow details = new StartScanWindow();
                    details.Owner = System.Windows.Application.Current.MainWindow;
                    if (details.ShowDialog() == true && message.Action != null)
                        message.Action(details.IsQuickScan);
                    break;
                case ChangeScreenTypes.StopScan:
                    ConfirmationWindow confirmation = new ConfirmationWindow();
                    confirmation.Owner = System.Windows.Application.Current.MainWindow;
                    if (confirmation.ShowDialog() == true && message.Action != null)
                        message.Action(null);
                    break;
            }
        }


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

            ScanCompletedWindow scanCompletedWindow = new ScanCompletedWindow(_viewModel);
            scanCompletedWindow.Text.Text = String.Empty;
            scanCompletedWindow.Text.Text += "Computer Statistics:";
            scanCompletedWindow.Text.Text += String.Format("{0}- {1} Total", Environment.NewLine, e.SuccededCount + e.CanceledCount + e.FailedCount + e.NotPerformedCount);
            scanCompletedWindow.Text.Text += Environment.NewLine;

            if (e.SuccededCount > 0) scanCompletedWindow.Text.Text += String.Format("- {0} Successful Scans", e.SuccededCount);
            if (e.CanceledCount > 0) scanCompletedWindow.Text.Text += String.Format("{0}- {1} Canceled Scans", Environment.NewLine, e.CanceledCount);
            if (e.FailedCount > 0) scanCompletedWindow.Text.Text += String.Format("{0}- {1} Failed Scans", Environment.NewLine, e.FailedCount);
            if (e.NotPerformedCount > 0) scanCompletedWindow.Text.Text += String.Format("{0}- {1} Not Performed Scans", Environment.NewLine, e.NotPerformedCount);

            scanCompletedWindow.Text.Text += Environment.NewLine +"File Statistics:";
            int total = e.FilesStatistics.UnknownFilesCount + e.FilesStatistics.MalwareFilesCount+ e.FilesStatistics.ErrorFilesCount + e.FilesStatistics.ServerErrorFilesCount;
            if (e.FilesStatistics.TotalFilesCound > 0)
            {
                scanCompletedWindow.Text.Text += String.Format("{0}- {1} Total(s) found", Environment.NewLine, total);
            }
            scanCompletedWindow.Text.Text += String.Format("{0}- {1} Unknown(s) found", Environment.NewLine, e.FilesStatistics.UnknownFilesCount);
            
            if (e.FilesStatistics.MalwareFilesCount > 0) scanCompletedWindow.Text.Text += String.Format("{0}- {1} Malware(s) found", Environment.NewLine, e.FilesStatistics.MalwareFilesCount);

            scanCompletedWindow.Text.Text += String.Format("{0}- {1} from {2} distinct by hash found", Environment.NewLine, e.DistinctFileFound, total);

            scanCompletedWindow.Owner = System.Windows.Application.Current.MainWindow;
            if (e.SuccededCount > 0)
            {
                if (scanCompletedWindow.ShowDialog() == true)
                    ShowRegisterToValkyrieWindow();
                else if (total == 0)
                    _viewModel.IsAnalyseWithValkyrieEnabled = false;
            }
                
        }
        
        private CamCredentialsWindow _dialog;
        private void ShowRegisterToValkyrieWindow()
        {
            var model = new ValkyrieRegistrationModel();

            _dialog = new CamCredentialsWindow(model);
            _dialog.Owner = System.Windows.Application.Current.MainWindow;

            _dialog.OkClick += (sender, args) =>
            {
                Dispatcher.Invoke((Action)delegate
                {
                    _dialog.loading.IsBusy = true;
                    _dialog.ok.Visibility = Visibility.Collapsed;
                });
                _dialog.MarkPasswordValidation(model.ValidatePassword());

                if (model.ValidateModel())
                {
                    var licid = _dialog.LicenseId;
                    var name = _dialog.UserName.TrimEnd();
                    var pass = _dialog.PasswordText.Password.TrimEnd();

                    
                    _viewModel.BeginValkyrieAnalysis(licid, name, pass);
                }
                else
                {
                    Dispatcher.Invoke((Action)delegate
                    {
                        _dialog.ok.Visibility = Visibility.Visible;
                        _dialog.loading.IsBusy = false;
                    });
                }
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
        private void ViewModelOnValkyrieAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<FileResult> e)
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

        private void OnPropertiesClick(Object sender, RoutedEventArgs e)
        {            
            ShowRegisterToValkyrieWindow();
        }



        private ThreatInfoItem GetDataBoundItem(FrameworkElement element)
        {
            if(element == null)
            {
                return null;
            }

            if(element.Parent == null)
            {
                return null;
            }

            FrameworkElement parent = (FrameworkElement) element.Parent;
            if(parent == null)
            {
                return null;
            }

            parent = (FrameworkElement) parent.Parent;
            if(parent == null)
            {
                return null;
            }

            return parent.DataContext as ThreatInfoItem;
        }

        private void OnTreeDoubleClick(Object sender, MouseButtonEventArgs e)
        {
            if(e.OriginalSource != null)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                if(element == null)
                {
                    return;
                }

                ThreatInfoItem item = GetDataBoundItem(element);
                if (_viewModel.SelectedItem != null && _viewModel.SelectedItem.IsRootItem == false && _viewModel.SelectedItem == item)
                {
                    ThreatDetailsWindow details = new ThreatDetailsWindow();
                    details.Owner = System.Windows.Application.Current.MainWindow;
                    details.ShowDialog();
                }
            }
        }

        private static void ShowReport()
        {
            ReportWindow details = new ReportWindow();
            details.Owner = System.Windows.Application.Current.MainWindow;
            details.ShowDialog();
        }

        private void OnExecutiveReportClick(Object sender, RadRoutedEventArgs e)
        {
            _viewModel.SetReportKind(ReportKind.Executive);
            ReportButton.IsOpen = false;
            ShowReport();
        }

        private void OnPerDeviceReportClick(Object sender, RadRoutedEventArgs e)
        {
            _viewModel.SetReportKind(ReportKind.DetailedPerDevice);
            ReportButton.IsOpen = false;
            ShowReport();
        }

        private void OnPerProgramReportClick(Object sender, RadRoutedEventArgs e)
        {
            _viewModel.SetReportKind(ReportKind.DetailedPerProgram);
            ReportButton.IsOpen = false;
            ShowReport();
        }

        private void OnExecutiveReportValkyrieClick(Object sender, RadRoutedEventArgs e)
        {
            _viewModel.SetReportKind(ReportKind.ExecutiveWithValkyrie);
            ReportButton.IsOpen = false;
            ShowReport();
        }

        private void OnPerDeviceReportValkyrieClick(Object sender, RadRoutedEventArgs e)
        {
           _viewModel.SetReportKind(ReportKind.DetailedPerDeviceWithValkyrie);
            ReportButton.IsOpen = false;
            ShowReport();
        }

        private void OnPerProgramReportValkyrieClick(Object sender, RadRoutedEventArgs e)
        {
            _viewModel.SetReportKind(ReportKind.DetailedPerProgramWithValkyrie);
            ReportButton.IsOpen = false;
            ShowReport();
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

        private void AboutClick(Object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow {Owner = System.Windows.Application.Current.MainWindow};
            about.ShowDialog();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
