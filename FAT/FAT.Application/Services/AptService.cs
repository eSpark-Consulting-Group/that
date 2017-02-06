using FAT.Application.Models;
using APTAT.Core;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FAT.Application.Models.Valkyrie;
using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;
using APTAT.Core.Reports;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.CamLicense;
using APTAT.Application.Services.EventArgs;
using APTAT.Application.Models.Apt;
using APTAT.Application.Properties;
using APTAT.Core.Apt;
using APTAT.Core.FLS;
using FAT.Application.Properties;

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
            #region old style
            _manager.ValkyrieAnalyzeCompleted += OnValkyrieAnalyzeCompleted;
            _manager.ValkyrieAnalyzeStarted += OnValkyrieAnalyzeStarted;
            _manager.ValkyrieAnalyzeProgressChanged += OnValkyrieAnalyzeProgressChanged;
            _manager.FlsAnalyzeProgressChanged += OnFlsAnalyzeProgressChanged;
            #endregion
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

        public GetModifiedComputerResultsForOperationEventArgs GetModifiedComputerResultsForOperation(Guid operationUid, DateTime? lastUtcTime)
        {
            var result = _manager.AggregatedScanResults;
            var completed = !result.IsOperationInProgress(operationUid);
            var mappedResult = result.ScanResults.Where(c => lastUtcTime == null || c.Modified > lastUtcTime)
                .Select(r => new ScanResultItem
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
                    BadFiles = r.BadFiles
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
                        }).ToArray()
                }).ToArray();
            return new GetModifiedComputerResultsForOperationEventArgs(mappedResult, completed);
        }

        public GetModifiedFileVerdictsEventArgs GetUpdatedFileVerdict(DateTime? lastFlcUtcTime, DateTime? lastValkyrieTime)
        {
            var result = _manager.GetModifiedFileVerdicts(lastFlcUtcTime);
            var valkyrieResult = _manager.GetModifiedValkyrieVerdicts(lastValkyrieTime);
            var mappedResult = result
                .Select(r => new FileVerdictContext(r.Sha1, r.Cloud, r.ModifiedDateTime)).ToList();
            mappedResult.AddRange(valkyrieResult.Select(vr => 
                new FileVerdictContext(vr.Sha1, vr.ValkyrieAnalysisResult, vr.VerdictModified)));
            return new GetModifiedFileVerdictsEventArgs(mappedResult.ToArray());
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
        #endregion


        #region Scan old style
        public event EventHandler<BeginScanCompletedEventArgs> BeginScanCompleted;
        public event EventHandler<ScanProgressChangedEventArgs> ScanProgressChanged;
        public event EventHandler<ScanStatusChangedEventArgs> ScanStatusChanged;
        public event EventHandler<GetScanResultCompletedEventArgs> ComputerScanCompleted;
        public event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        public event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieFile>> ValkyrieAnalyzeProgressChanged;
        public event AnalyzeCompletedEventHandler<ValkyrieFile> ValkyrieAnalyzeCompleted;
        public event EventHandler ValkyrieAnalyzeStarted;
        public event EventHandler<FlsRequestEventArgs<IAptVerdictContext>> FlsAnalyzeProgressChanged;
        

        private void OnFlsAnalyzeProgressChanged(object sender, FlsRequestEventArgs<IAptVerdictContext> e)
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

        public void BeginScan(DeploymentTarget[] targets, Object userState, 
            IDevicesSourceDescription devicesSourceDescription, ScanOptions options)
        {
            IAptScanner scanner = _manager.GetAptScanner(targets, devicesSourceDescription, options);
            scanner.ScanStarted += OnScanStarted;
            scanner.ComputerScanCompleted += OnComputerScanCompleted;
            scanner.ScanInProgress += OnScanInProgress;
            scanner.ScanCompleted += OnScanCompleted;
            scanner.BeginScan(userState);
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
                    progressItem.OperationUid = aptScantProgress.OperationUid;
                    progressItem.ProgressPercentage = aptScantProgress.ProgressPercentage;
                    progressItem.ScanStage = FromAptScanStage(aptScantProgress.ScanStage);
                }
                else
                {
                    progressItem.ComputerGuid = descentProgressItem.ComputerUid;
                    progressItem.StatusMessage = descentProgressItem.StatusMessage.ToString();
                    progressItem.OperationUid = descentProgressItem.OperationUid;
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
                BeginScanCompletedEventArgs args = new BeginScanCompletedEventArgs(e.OperationUid, e.State, e.Error);
                BeginScanCompleted?.Invoke(this, args);
            };
            App.Current.Dispatcher.BeginInvoke(action);
        }

        private void OnScanCompleted(Object sender, APTAT.Core.ScanCompletedEventArgs e)
        {
            var scanner = sender as IAptScanner;
            if (scanner != null)
            {
                scanner.ScanStarted -= OnScanStarted;
                scanner.ScanCompleted -= OnScanCompleted;
                scanner.ScanInProgress -= OnScanInProgress;
                scanner.ComputerScanCompleted -= OnComputerScanCompleted;
            }
            //Logger.WriteLog("Scan Completed");
            ScanCompletedEventArgs args = new ScanCompletedEventArgs();
            args.SuccededCount = e.SucceededCount;
            args.CanceledCount = e.CanceledCount;
            args.FailedCount = e.FailedCount;
            args.NotPerformedCount = e.NotPerformedCount;
            var fs = _manager.AggregatedScanResults.CalculateFilesStatistics();
            args.FilesStatistics = fs;
            args.DistinctFileFound = _manager.GetDistinctFilesCount();

            Action action = () =>
            {
                ScanCompleted?.Invoke(this, args);
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

        public void BeginValkyrieAnalysis(string systemLogin, string systemPassword
            , string camLicenseId, string camUserName, string camPassword,
            NetworkCredential networkCredential)
        {

            Action action = () =>
            {
                if (string.IsNullOrEmpty(camLicenseId) && string.IsNullOrEmpty(camUserName) && string.IsNullOrEmpty(camPassword))

                    try
                    {
                        camLicenseId = _camLicense ??
                                       (_camLicense = _camLicenseProvider.TryGetFreeLicense(networkCredential));
                    }
                    catch (Exception e)
                    {
                        
                        Action a = () =>
                        {
                            OnValkyrieAnalyzeCompleted(this,
                                new AnalyzeCompletedEventArgs<FileResult>(new FileResult[] { }, e, false, null));
                        };
                        App.Current.Dispatcher.BeginInvoke(a);
                        return;
                    }

                
                _manager.StartValkyrieAnalysis(systemLogin, systemPassword, camLicenseId, camUserName,
                    camPassword, networkCredential, "FAT");
            };

            Task.Factory.StartNew(action, TaskCreationOptions.None);
            
            
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
                _manager.ValkyrieAnalyzeCompleted -= OnValkyrieAnalyzeCompleted;
                _manager.ValkyrieAnalyzeProgressChanged -= OnValkyrieAnalyzeProgressChanged;
                _manager.ValkyrieAnalyzeStarted -= OnValkyrieAnalyzeStarted;

                _manager.ValkyrieAnalyzeCompleted -= OnValkyrieAnalyzeCompleted2;
                _manager.Dispose();
                _manager = null;
            }
        }
        #endregion
    }
}
