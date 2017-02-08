using FAT.Application.Models;
using APTAT.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;
using APTAT.Core.Reports;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.CamLicense;
using APTAT.Application.Services.EventArgs;
using APTAT.Application.Models.Apt;
using APTAT.Application.Properties;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;

namespace FAT.Application.Services
{

    public class AptService : IAptService
    {
        private IAptAtManager _manager;
        private readonly ICamLicenseProvider _camLicenseProvider;
        private string _camLicense = null;
        private readonly Guid _sessionGuid = new Guid("C78DA9FA-4DC3-4F26-B7C3-304D24220B15");

        private string _systemLogin;
        private string _systemPassword;
        private bool _valkyrieStarted = false;
        public Guid SessionId
        {
            get { return _sessionGuid; }
        }
        public AptService(IAptAtManager aptAtManager, ICamLicenseProvider camLicenseProvider)
        {
            _camLicenseProvider = camLicenseProvider;
            _manager = aptAtManager;

            #region new style
            _manager.ValkyrieAnalyzeCompleted += OnValkyrieAnalyzeCompleted2;
            #endregion
        }
        #region reporting
        public event EventHandler<BuildReportCompletedEventArgs> BuildReportCompleted;
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
                var builder = (IAptReportBuilder)sender;
                builder.ReportCompleted -= OnReportCompleted;
            }
            Action action = () =>
            {
                BuildReportCompleted?.Invoke(this, args);
            };
            App.Current.Dispatcher.BeginInvoke(action);
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
        #endregion
        #region Scan for FAT
        public event EventHandler<ScanStartedEventArgs> FatScanStarted;
        public event EventHandler<ErrorEventArgs> ErrorOccurred;
        public event EventHandler<ErrorEventArgs> FatScanCompleted;

        public void BeginScan(DeploymentTarget[] targets, IDevicesSourceDescription devicesSourceDescription,
            ScanOptions options)
        {
            IAptScanner scanner = _manager.GetAptScanner(targets, devicesSourceDescription, options);
            _systemLogin = options.Login;
            _systemPassword = options.Password;
            scanner.ScanStarted += OnScanStarted2;
            scanner.ComputerScanCompleted += OnComputerScanCompleted2;
            scanner.ScanCompleted += OnScanCompleted2;
            scanner.BeginScan();
        }

        private void OnScanCompleted2(Object sender, APTAT.Core.ScanCompletedEventArgs e)
        {
            var scanner = sender as IAptScanner;
            if (scanner != null)
            {
                scanner.ScanCompleted -= OnScanCompleted2;
                scanner.ScanStarted -= OnScanStarted2;
                scanner.ComputerScanCompleted -= OnComputerScanCompleted2;
            }
            lock (this)
            {
                if(!_valkyrieStarted)
                    FatScanCompleted?.Invoke(this, new ErrorEventArgs(null));
            }
        }


        private void OnComputerScanCompleted2(Object sender, ComputerScanCompletedEventArgs e)
        {
            Action action = () =>
            {
                lock (this)
                {
                    if (_valkyrieStarted) return;
                    string camLicenseId;
                    try
                    {
                        camLicenseId = _camLicense ??
                                       (_camLicense = _camLicenseProvider.TryGetFreeLicense(null));
                    }
                    catch (Exception ex)
                    {
                        Action a = () =>
                        {
                            ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex));
                        };
                        App.Current.Dispatcher.BeginInvoke(a);
                        return;
                    }


                    _manager.StartValkyrieAnalysis(_systemLogin, _systemPassword, camLicenseId, null,
                        null, null, Settings.Default.applicationName);
                    _valkyrieStarted = true;
                }
            };
            if (e.CompletedResult.SafeFilesCount != e.CompletedResult.TotalFilesCount)
            {
                Task.Factory.StartNew(action, TaskCreationOptions.None);
                var scanner = sender as IAptScanner;
                if (scanner != null) scanner.ComputerScanCompleted -= OnComputerScanCompleted2;
            }
        }

        public GetModifiedResultsForOperationEventArgs GetModifiedResultsForOperation(Guid operationUid, DateTime? lastUtcTime,
            DateTime? lastFlcUtcTime, DateTime? lastValkyrieTime)
        {
            var result = _manager.AggregatedScanResults;
            var completed = !result.IsOperationInProgress(operationUid);
            var percentage = GetProgressOfOperations(operationUid) / 2;
            List<ScanResultItem> mappedResult = new List<ScanResultItem>();
            Dictionary<string,IModule> modules = new Dictionary<string,IModule>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in result.ScanResults.Where(c => lastUtcTime == null || c.Modified > lastUtcTime))
            {
                List<BadFileItem> badfiles = new List<BadFileItem>();
                foreach (var m in r.BadFiles)
                {
                    if (!modules.ContainsKey(m.Sha1)&&(m.IsUnknown() || m.Cloud == CloudVerdict.Malware))
                        modules.Add(m.Sha1, m);
                    badfiles.Add(new BadFileItem
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
                    });
                }
                mappedResult.Add(new ScanResultItem
                {
                    TargetUid = r.TargetUid,
                    TotalFilesCount = r.TotalFilesCount,
                    Percentage = r.Percentage,
                    MalwareFilesCount = r.MalwareFilesCount,
                    NoVerdictFilesCount = r.NoVerdictFilesCount,
                    FailedToAnalyzeFilesCount = r.FailedToAnalyzeFilesCount,
                    UnknownFilesCount = r.UnknownFilesCount,
                    CleanFilesCount = r.SafeFilesCount,
                    ScanStage = FromAptScanStage(r.ScanStage),
                    Modified = r.Modified,
                    StatusMessage = r.ShortStatusMessage.ToString(),
                    BadFiles = badfiles.ToArray()
                });
            }

            var files = _manager.GetModifiedFileVerdicts(lastFlcUtcTime);
            Dictionary<string, ValkyrieAnalysisResult> valkyrieAnalysisResults;
            var valkyrieResult = _manager.GetModifiedValkyrieVerdicts(lastValkyrieTime, modules, out valkyrieAnalysisResults);
            var mappedFiles = files
                .Select(r => new FileVerdictContext(r.Sha1, r.Cloud, r.ModifiedDateTime)).ToList();
            mappedFiles.AddRange(valkyrieResult.Select(vr =>
                new FileVerdictContext(vr.Sha1, vr.ValkyrieAnalysisResult, vr.VerdictModified)));
            mappedFiles.AddRange(valkyrieAnalysisResults.Values.Select(vr =>
                new FileVerdictContext(vr.SHA1, vr, DateTime.MinValue)));
            return new GetModifiedResultsForOperationEventArgs(mappedResult.ToArray(), mappedFiles.ToArray(), completed, percentage);
        }

        public double GetProgressOfOperations(Guid operationUid)
        {
            var scanResults = _manager.AggregatedScanResults.ScanResults.OfType<IAptScanResult>()
                .Where(r => r.ResultType != APTAT.Core.ScanResultType.NotPerformed).ToArray();
            var result = scanResults.Sum(sr => sr.Percentage) / (scanResults.Count() * 100) * 100d;
            return result;
        }

        void OnValkyrieAnalyzeCompleted2(object sender, AnalyzeCompletedEventArgs<FileResult> e)
        {
            Action action;
            if (e.Error != null)
            { 
                action = () =>
                {
                    ErrorOccurred?.Invoke(this, new ErrorEventArgs(e.Error));
                };
            }
            else
            {
                action = () =>
                {
                    FatScanCompleted?.Invoke(this, new ErrorEventArgs(null));
                };
            }

            App.Current.Dispatcher.BeginInvoke(action);
        }
        private void OnScanStarted2(Object sender, ScanStartedEventArgs e)
        {
            _valkyrieStarted = false;
            var scanner = sender as IAptScanner;
            if (scanner != null)
            {
                scanner.ScanStarted -= OnScanStarted2;
            }
            Action action = () =>
            {
                ScanStartedEventArgs args = new ScanStartedEventArgs
                {
                    OperationUid = e.OperationUid,
                    RescanAttempt = e.RescanAttempt,
                    Error = e.Error
                };
                FatScanStarted?.Invoke(this, args);
            };
            App.Current.Dispatcher.BeginInvoke(action);
        }
        public void StopScan(Guid[] targets)
        {
            _manager.StopScan(targets);
        }

        public void UpdateFlsPorts(int udpPort, int tcpPort)
        {
            _manager.UpdateFlsPorts(udpPort, tcpPort);

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
        #endregion


        #region IDisposable
        public void Dispose()
        {
            if (_manager != null)
            {
                _manager.ValkyrieAnalyzeCompleted -= OnValkyrieAnalyzeCompleted2;
                _manager.Dispose();
                _manager = null;
            }
        }
        #endregion
    }
}
