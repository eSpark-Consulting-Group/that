using APTAT.Application.Models;
using APTAT.Core;
using APTAT.Core.Descent;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using System;
using System.Linq;
using System.ComponentModel;
using System.Net;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Valkyrie;
using APTAT.Core.Reports;

using Composition = APTAT.Application.Helpers.CompositionHelper;

namespace APTAT.Application.Services
{
    [Composition.CreationPolicy(Composition.CreationPolicy.Singleton)]
    public class AptService : IAptService
    {
        public event EventHandler<BeginScanCompletedEventArgs> BeginScanCompleted;

        public event EventHandler<ScanProgressChangedEventArgs> ScanProgressChanged;

        public event EventHandler<ScanStatusChangedEventArgs> ScanStatusChanged;

        public event EventHandler<GetScanResultCompletedEventArgs> GetScanResultCompleted;

        public event EventHandler<ScanCompletedEventArgs> ScanCompleted;

        public event EventHandler<BuildReportCompletedEventArgs> BuildReportCompleted;

        public event ProgressChangedEventHandler ValkyrieAnalyzeProgressChanged;
        public event AnalyzeCompletedEventHandler<FileResult> ValkyrieAnalyzeCompleted;
        public event EventHandler ValkyrieAnalyzeStarted;



        private readonly AptAtManager _manager;


        private readonly Guid _sessionGuid = new Guid("C78DA9FA-4DC3-4F26-B7C3-304D24220B15");

        public Guid SessionId
        {
            get { return _sessionGuid; }
        }


        public AptService()
        {
            _manager = new AptAtManager();

        }
        void OnValkyrieAnalyzeStarted(object sender, EventArgs e)
        {
            Action action = () =>
            {
                var handler = ValkyrieAnalyzeStarted;
                if (handler != null)
                {
                    handler(this, e);
                }
            };
            App.Current.Dispatcher.BeginInvoke(action);
        }

        void OnValkyrieAnalyzeProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Action action = () =>
            {
                var handler = ValkyrieAnalyzeProgressChanged;
                if (handler != null)
                {
                    handler(this, e);
                }
            };
            App.Current.Dispatcher.BeginInvoke(action);
        }

        void OnValkyrieAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<FileResult> e)
        {
            _manager.ValkyrieAnalyzeCompleted -= OnValkyrieAnalyzeCompleted;
            _manager.ValkyrieAnalyzeProgressChanged -= OnValkyrieAnalyzeProgressChanged;
            _manager.ValkyrieAnalyzeStarted -= OnValkyrieAnalyzeStarted;
            Action action = () =>
            {
                var handler = ValkyrieAnalyzeCompleted;
                if (handler != null)
                {
                    handler(this, e);
                }
            };
            App.Current.Dispatcher.BeginInvoke(action);
        }



        public void BuildReport(Guid operationGuid, ReportKind reportKind, Object userState)
        {
            IAptReportBuilder reportBuilder = _manager.GetAptReportBuilder();
            reportBuilder.ReportCompleted += OnReportCompleted;

            reportBuilder.BeginBuildReport(operationGuid, ToReportType(reportKind), userState);
        }

        private void OnReportCompleted(Object sender, ReportCompletedEventArgs e)
        {
            BuildReportCompletedEventArgs args = new BuildReportCompletedEventArgs(e.FileName, e.Error, e.State);

            Action action = () =>
            {
                if (BuildReportCompleted != null)
                {
                    BuildReportCompleted(this, args);
                }
            };
            App.Current.Dispatcher.BeginInvoke(action);


        }

        public void BeginScan(DeploymentTarget[] items , String login, String password, Object userState, string scanScope)
        {
            IAptScanner scanner = _manager.GetAptScanner(new DeploymentTargetsCollection(items), login, password, scanScope);
            scanner.ScanStarted += OnScanStarted;
            scanner.ScanCompleted += OnScanCompleted;
            scanner.ScanInProgress += OnScanInProgress;
            scanner.BeginScan(userState);
        }

        private void OnScanInProgress(Object sender, AptScanInProgressEventArgs e)
        {
            ScanProgressChangedEventArgs args = new ScanProgressChangedEventArgs(e.Percentage, e.OperationUid);
            ScanProgressItem[] scanProgressItems = new ScanProgressItem[e.ProgressItems.Length];
            for (var i = 0; i < e.ProgressItems.Length; ++i)
            {
                DescentProgressItem descentProgressItem = e.ProgressItems[i];
                if (descentProgressItem != null)
                {
                    ScanProgressItem progressItem = new ScanProgressItem();
                    progressItem.ComputerGuid = descentProgressItem.ComputerUid;
                    progressItem.StatusMessage = descentProgressItem.StatusMessage.ToString();
                    progressItem.TotalFilesCount = descentProgressItem.TotalFilesCount;
                    progressItem.UnknownFilesCount = descentProgressItem.UnknownFilesCount;
                    progressItem.MalwareFilesCount = descentProgressItem.MalwaresCount;
                    progressItem.ErrorFilesCount = descentProgressItem.ErrorFilesCount;
                    progressItem.ServerErrorFilesCount = descentProgressItem.FailedAtNodeCount;
                    progressItem.OperationUid = descentProgressItem.OperationUid;
                    progressItem.ScanStage = FromAptScanStage(descentProgressItem.ScanStage);
                    scanProgressItems[i] = progressItem;
                }
            }

            Action action = () =>
            {

                if (ScanStatusChanged != null)
                {
                    ScanStatusChanged(this, new ScanStatusChangedEventArgs(scanProgressItems, null));
                }
                if (ScanProgressChanged != null)
                {
                    ScanProgressChanged(this, args);
                }
            };
            App.Current.Dispatcher.BeginInvoke(action);
        }

        private void OnScanStarted(Object sender, ScanStartedEventArgs e)
        {
            //Logger.WriteLog("Scan Started");

            BeginScanCompletedEventArgs args = new BeginScanCompletedEventArgs(e.OperationUid, e.State, e.Error);

            if (BeginScanCompleted != null)
            {
                BeginScanCompleted(this, args);

            }

        }

        private void OnScanCompleted(Object sender, Core.ScanCompletedEventArgs e)
        {
            var scanner = sender as IAptScanner;
            if (scanner != null)
            {
                scanner.ScanStarted -= OnScanStarted;
                scanner.ScanCompleted -= OnScanCompleted;
                scanner.ScanInProgress -= OnScanInProgress;
            }
            //Logger.WriteLog("Scan Completed");
            ScanCompletedEventArgs args = new ScanCompletedEventArgs();
            args.SuccededCount = e.SucceededCount;
            args.CanceledCount = e.CalceledCount;
            args.FailedCount = e.FailedCount;
            args.NotPerformedCount = e.NotPerformedCount;
            args.FilesStatistics = e.FilesStatistics;
            
            args.MalwaresFound = e.FilesStatistics.MalwareFilesCount;

            int distCount = _manager.GetDistinctFileCount();
            args.DistinctFileFound = distCount;
            //AptAtManager.GetDistinctFileInfo();

            Action action = () =>
            {
                if (ScanCompleted != null)
                {
                    ScanCompleted(this, args);
                }
            };
            App.Current.Dispatcher.BeginInvoke(action);

        }

        public void GetScanResult(Guid targetUid, Object userState)
        {
            AptScanResult aptScanResult = _manager.GetScanResultByTargetUid(targetUid);

            ScanResultItem resultItem = new ScanResultItem
            {
                TargetUid = aptScanResult.TargetUid,
                TotalFilesCount = aptScanResult.TotalFilesCount,
                UnknownFilesCount = aptScanResult.UnknownFilesCount
            };


            var badFileItems = aptScanResult.BadFiles
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
                    IsMalware = m.Cloud == CloudVerdict.Malware,
                    IsUnknown = m.Cloud == CloudVerdict.Unknown || m.Cloud == CloudVerdict.FirstSeen
                }
                ).ToArray();


            resultItem.BadFiles = badFileItems;


            if (GetScanResultCompleted != null)
            {
                GetScanResultCompleted(this, new GetScanResultCompletedEventArgs(resultItem, userState));
            }
        }

        public void StopScan()
        {
            _manager.StopScan();
        }

        

        public void BeginValkyrieAnalysis(string systemLogin, string systemPassword
            , string camLicenseId, string camUserName, string camPassword,
            NetworkCredential networkCredential = null)
        {
            _manager.ValkyrieAnalyzeCompleted += OnValkyrieAnalyzeCompleted;
            _manager.ValkyrieAnalyzeProgressChanged += OnValkyrieAnalyzeProgressChanged;
            _manager.ValkyrieAnalyzeStarted += OnValkyrieAnalyzeStarted;
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
    }
}
