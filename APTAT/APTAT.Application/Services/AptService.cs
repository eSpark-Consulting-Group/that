using APTAT.Application.Models;
using APTAT.Core;
using System;
using System.Linq;
using System.Net;
using APTAT.Application.Models.Valkyrie;
using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;
using APTAT.Core.Reports;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;



namespace APTAT.Application.Services
{

    public class AptService : IAptService
    {
        public event EventHandler<BeginScanCompletedEventArgs> BeginScanCompleted;
        public event EventHandler<ScanProgressChangedEventArgs> ScanProgressChanged;
        public event EventHandler<ScanStatusChangedEventArgs> ScanStatusChanged;
        public event EventHandler<GetScanResultCompletedEventArgs> ComputerScanCompleted;
        public event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        public event EventHandler<BuildReportCompletedEventArgs> BuildReportCompleted;
        public event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieFile>> ValkyrieAnalyzeProgressChanged;
        public event AnalyzeCompletedEventHandler<ValkyrieFile> ValkyrieAnalyzeCompleted;
        public event EventHandler ValkyrieAnalyzeStarted;
        public event EventHandler<FlsRequestEventArgs> FlsAnalyzeProgressChanged;
        private IAptAtManager _manager;
        private readonly Guid _sessionGuid = new Guid("C78DA9FA-4DC3-4F26-B7C3-304D24220B15");
        public Guid SessionId
        {
            get { return _sessionGuid; }
        }
        public AptService(IAptAtManager aptAtManager)
        {
            _manager = aptAtManager;
            _manager.ValkyrieAnalyzeCompleted += OnValkyrieAnalyzeCompleted;
            _manager.ValkyrieAnalyzeProgressChanged += OnValkyrieAnalyzeProgressChanged;
            _manager.ValkyrieAnalyzeStarted += OnValkyrieAnalyzeStarted;
            _manager.FlsAnalyzeProgressChanged += OnFlsAnalyzeProgressChanged;

        }

        private void OnFlsAnalyzeProgressChanged(object sender, FlsRequestEventArgs e)
        {
            Action action = () =>
            {
                FlsAnalyzeProgressChanged?.Invoke(this, e);
            };
            System.Windows.Application.Current.Dispatcher.BeginInvoke(action);
        }

        
        void OnValkyrieAnalyzeStarted(object sender, EventArgs e)
        {
            Action action = () =>
            {
                ValkyrieAnalyzeStarted?.Invoke(this, e);
            };
            App.Current.Dispatcher.BeginInvoke(action);
        }

        void OnValkyrieAnalyzeProgressChanged(object sender, ValkyrieAnalyzeProgressChangedEventArgs<FileResult> e)
        {
            Action action = () =>
            {
                ValkyrieAnalyzeProgressChanged?.Invoke(this,e.ToClient());
            };
            App.Current.Dispatcher.Invoke(action);
        }

        void OnValkyrieAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<FileResult> e)
        {
            Action action = () =>
            {
                ValkyrieAnalyzeCompleted?.Invoke(this,e.ToClient());
            };
            App.Current.Dispatcher.BeginInvoke(action);
        }



        public void BuildReport(Guid operationGuid, ReportKind reportKind, Object userState)
        {
            IAptReportBuilder reportBuilder = _manager.GetAptReportBuilder();
            reportBuilder.ReportCompleted += OnReportCompleted;
            reportBuilder.BeginLastBuildReport(ToReportType(reportKind), userState);
        }

        private void OnReportCompleted(Object sender, ReportCompletedEventArgs e)
        {
            BuildReportCompletedEventArgs args = new BuildReportCompletedEventArgs(e.FileName, e.Error, e.State);
            if (sender is IAptReportBuilder)
            {
                var builder = (IAptReportBuilder) sender;
                builder.ReportCompleted -= OnReportCompleted;
            }
            Action action = () =>
            {
                BuildReportCompleted?.Invoke(this,args);                   
            };
            App.Current.Dispatcher.BeginInvoke(action);
        }

        public void BeginScan(DeploymentTarget[] targets, String login, String password, Object userState, 
            IDevicesSourceDescription devicesSourceDescription, ScanOptions options)
        {
            IAptScanner scanner = _manager.GetAptScanner(devicesSourceDescription, options);

            if (scanner.ScanOperationContext == null)
            {
                scanner.ScanStarted += OnScanStarted;
                scanner.ComputerScanCompleted += OnComputerScanCompleted;
                scanner.ScanInProgress += OnScanInProgress;
                scanner.ScanCompleted += OnScanCompleted;
            }

            scanner.BeginScan(targets, login, password, scanner.ScanOperationContext != null, userState);
        }

        private void OnComputerScanCompleted(Object sender, ComputerScanCompletedEventArgs e)
        {
            ScanResultItem resultItem = new ScanResultItem
            {
                TargetUid = e.CompletedResult.TargetUid,
                TotalFilesCount = e.CompletedResult.TotalFilesCount,
                UnknownFilesCount = e.CompletedResult.UnknownFilesCount,
                ScanStage = FromAptScanStage(e.CompletedResult.ScanStage),
                StatusMessage = e.CompletedResult.ShortStatusMessage.ToString()
            };
            var badFileItems = e.CompletedResult.BadFiles
                .Select(m => new BadFileItem
                {
                    Name = m.Path,
                    Size = m.Size,
                    Accessed = m.Accessed,
                    Created = m.Created,
                    Modified = m.Modified,
                    Product = m.Product,
                    Copyright = m.Copyright,
                    Version = m.Version,
                    Description = m.Description,
                    Signer = m.Signer,
                    Attributes = m.Attributes,
                    Sha1 = m.Sha1,
                    Cloud = m.Cloud,
                }).ToArray();
            resultItem.BadFiles = badFileItems;
            Action action = () =>
            {
                ComputerScanCompleted?.Invoke(this, new GetScanResultCompletedEventArgs(resultItem));
            };
            App.Current.Dispatcher.BeginInvoke(action);
        }

        private void OnScanInProgress(Object sender, AptScanInProgressEventArgs e)
        {
            ScanProgressChangedEventArgs args = new ScanProgressChangedEventArgs(e.Percentage, e.OperationUid);
            ScanProgressItem[] scanProgressItems = new ScanProgressItem[e.ProgressItems.Length];
            for (var i = 0; i < e.ProgressItems.Length; ++i)
            {
                IDescentProgressItem descentProgressItem = e.ProgressItems[i];
                var aptScantProgress = descentProgressItem as IAptDescentProgressItem;
                ScanProgressItem progressItem = new ScanProgressItem();
                scanProgressItems[i] = progressItem;
                if (aptScantProgress != null)
                {
                    progressItem.ComputerGuid = aptScantProgress.ComputerUid;
                    progressItem.StatusMessage = aptScantProgress.StatusMessage.ToString();

                    progressItem.TotalFilesCount = aptScantProgress.TotalFilesCount;
                    progressItem.UnknownFilesCount = aptScantProgress.UnknownFilesCount;
                    progressItem.MalwareFilesCount = aptScantProgress.MalwaresCount;
                    progressItem.FailedToAnalyzeFilesCount = aptScantProgress.ErrorFilesCount;
                    progressItem.NoVerdictFilesCount = aptScantProgress.FailedAtNodeCount;
                    progressItem.OperationUid = e.OperationUid;
                    progressItem.ProgressPercentage = aptScantProgress.ProgressPercentage;
                    progressItem.ScanStage = FromAptScanStage(aptScantProgress.ScanStage);
                }
                else
                {
                    progressItem.ComputerGuid = descentProgressItem.ComputerUid;
                    progressItem.StatusMessage = descentProgressItem.StatusMessage.ToString();
                    progressItem.OperationUid = e.OperationUid;
                    progressItem.ScanStage = descentProgressItem.IsCanceled ? 
                        ScanStage.OperationCanceled : ScanStage.OperationFailed;
                }
            }

            Action action = () =>
            {
                ScanStatusChanged?.Invoke(this, new ScanStatusChangedEventArgs(scanProgressItems, null));
                ScanProgressChanged?.Invoke(this, args);
            };
            App.Current.Dispatcher.BeginInvoke(action);
        }

        private void OnScanStarted(Object sender, ScanStartedEventArgs e)
        {
            Action action = () =>
            {
                BeginScanCompletedEventArgs args = new BeginScanCompletedEventArgs(e.OperationUid, e.RescanAttempt, e.State, e.Error);
                BeginScanCompleted?.Invoke(this, args);
            };
            App.Current.Dispatcher.BeginInvoke(action);
        }

        private void OnScanCompleted(Object sender, Core.ScanCompletedEventArgs e)
        {
            var scanner = sender as IAptScanner;
            if (scanner != null)
            {
                scanner.ScanStarted -= OnScanStarted;
                scanner.ScanCompleted -= OnScanCompleted;
                scanner.ScanInProgress -= OnScanInProgress;
                scanner.ComputerScanCompleted -= OnComputerScanCompleted;
            }

            ScanCompletedEventArgs args = new ScanCompletedEventArgs();
            args.SuccededCount = e.SucceededCount;
            args.CanceledCount = e.CanceledCount;
            args.FailedCount = e.FailedCount;
            args.NotPerformedCount = e.NotPerformedCount;
            args.FilesStatistics = e.FilesStatistics;
            args.MalwaresFound = e.FilesStatistics.MalwareFilesCount;
            args.DistinctFileFound = _manager.GetDistinctFilesCount();


            Action action = () =>
            {
                ScanCompleted?.Invoke(this, args);
            };
            App.Current.Dispatcher.BeginInvoke(action);

        }

        public void UpdateFlsPorts(int udpPort, int tcpPort)
        {
            _manager.UpdateFlsPorts(udpPort, tcpPort);

        }

        public void StopScan(Guid operation, Guid[] targets)
        {
            _manager.StopScan(operation, targets);
        }

        

        public void BeginValkyrieAnalysis(string systemLogin, string systemPassword
            , string camLicenseId, string camUserName, string camPassword,
            NetworkCredential networkCredential)
        {
            _manager.StartValkyrieAnalysis(systemLogin, systemPassword, camLicenseId, camUserName, 
                camPassword, networkCredential);
        }

        public string GetValkyrieDetailsLink()
        {
            return _manager.GetValkyrieDetailsLink();
        }

        

        private ScanStage FromAptScanStage(AptScanStage stage)
        {
            switch (stage)
            {
                case AptScanStage.Pending:
                    return ScanStage.Pending;

                case AptScanStage.Connecting:
                    return ScanStage.Connecting;

                case AptScanStage.CopyingScanner:
                    return ScanStage.CopyingScanner;

                case AptScanStage.ConfiguringScanner:
                    return ScanStage.ConfiguringScanner;

                case AptScanStage.StartingScanner:
                    return ScanStage.StartingScanner;

                case AptScanStage.ScanInProgress:
                    return ScanStage.ScanInProgress;

                case AptScanStage.ScanCompleted:
                    return ScanStage.ScanCompleted;

                case AptScanStage.ScanFailed:
                    return ScanStage.ScanFailed;

                case AptScanStage.ScanResultsObtaning:
                    return ScanStage.ScanResultsObtaning;

                case AptScanStage.ScanResultsProcessing:
                    return ScanStage.ScanResultsProcessing;

                case AptScanStage.DescentOperationCompleted:
                    return ScanStage.DescentOperationCompleted;

                case AptScanStage.OperationCompleted:
                    return ScanStage.OperationCompleted;

                case AptScanStage.OperationFailed:
                    return ScanStage.OperationFailed;

                case AptScanStage.OperationCanceled:
                    return ScanStage.OperationCanceled;

                default:
                    return ScanStage.Unknown;
                //  throw new ArgumentOutOfRangeException("stage");
            }
        }

        

        private ReportType ToReportType(ReportKind kind)
        {
            if (kind == ReportKind.Executive)
            {
                return ReportType.Executive;
            }

            if (kind == ReportKind.DetailedPerProgram)
            {
                return ReportType.DetailedPerProgram;
            }

            if (kind == ReportKind.DetailedPerDevice)
            {
                return ReportType.DetailedPerDevice;
            }

            if (kind == ReportKind.ExecutiveWithValkyrie)
            {
                return ReportType.ExecutiveWithValkyrie;
            }

            if (kind == ReportKind.DetailedPerProgramWithValkyrie)
            {
                return ReportType.DetailedPerProgramWithValkyrie;
            }

            if (kind == ReportKind.DetailedPerDeviceWithValkyrie)
            {
                return ReportType.DetailedPerDeviceWithValkyrie;
            }

            throw new ArgumentException("kind");
        }

        public void Dispose()
        {
            if (_manager != null)
            {
                _manager.ValkyrieAnalyzeCompleted -= OnValkyrieAnalyzeCompleted;
                _manager.ValkyrieAnalyzeProgressChanged -= OnValkyrieAnalyzeProgressChanged;
                _manager.ValkyrieAnalyzeStarted -= OnValkyrieAnalyzeStarted;
                _manager.Dispose();
                _manager = null;
            }
        }
    }
}
