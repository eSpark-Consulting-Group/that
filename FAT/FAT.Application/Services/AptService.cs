using FAT.Application.Models;
using APTAT.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Reports;
using APTAT.DeploymentCore.Directory;
using APTAT.Core.CamLicense;
using APTAT.Application.Services.EventArgs;
using APTAT.Application.Models.Apt;
using APTAT.Core.Valkyrie;
using APTAT.DeploymentCore;
using FAT.Application.Properties;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;

namespace FAT.Application.Services
{
    public class AptService : IAptService
    {
        private IAptAtManager _manager;
        private readonly ICamLicenseProvider _camLicenseProvider;
        private readonly IProxySettingsViewModel _proxyCredentialsProvider;
        private string _camLicense = null;
        private NetworkCredential _proxyCredentials;

        private string _systemLogin;
        private string _systemPassword;
        private bool _isValkyrieStarted = false;
        public Guid SessionId { get; } = new Guid("C78DA9FA-4DC3-4F26-B7C3-304D24220B15");

        

        public AptService(IAptAtManager aptAtManager, ICamLicenseProvider camLicenseProvider
            , IProxySettingsViewModel settingsViewModel)
        {
            _camLicenseProvider = camLicenseProvider;
            _manager = aptAtManager;
            _proxyCredentialsProvider = settingsViewModel;

            #region new style
            _manager.ValkyrieAnalyzeCompleted += OnValkyrieAnalyzeCompleted;
            #endregion
        }
        #region reporting
        public async Task<BuildReportResult> BuildReportAsync(ReportKind reportKind)
        {
            var result = await _manager.BuildReportAsync(ToReportType(reportKind), false);
            return new BuildReportResult(result.FileName, result.DocumentBytes, result.Error);                        
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
        public event EventHandler<ErrorEventArgs> FatScanCompleted;
        public event EventHandler CancelScanStarted;
        public void BeginScan(IComputer[] targets, IDevicesSourceDescription devicesSourceDescription,
            ScanOptions options)
        {
            lock (this)
            {
                _isValkyrieStarted = false;
            }
            IAptScanner scanner = _manager.GetAptScanner(targets, devicesSourceDescription, options);
            _systemLogin = options.Login;
            _systemPassword = options.Password;
            scanner.ScanStarted += OnScanStarted;
            scanner.ComputerScanCompleted += OnComputerScanCompleted;
            scanner.ScanCompleted += OnScanCompleted;
            scanner.BeginScan();
        }

        private void OnScanCompleted(Object sender, APTAT.Core.ScanCompletedEventArgs e)
        {
            var scanner = sender as IAptScanner;
            if (scanner != null)
            {
                scanner.ScanCompleted -= OnScanCompleted;
                scanner.ScanStarted -= OnScanStarted;
                scanner.ComputerScanCompleted -= OnComputerScanCompleted;
                scanner.Dispose();
            }
            lock (this)
            {
                if (!_isValkyrieStarted)
                {
                    Action a = () =>
                    {
                        FatScanCompleted?.Invoke(this, new ErrorEventArgs(null));
                    };
                    App.Current.Dispatcher.BeginInvoke(a);
                }
            }
        }


        private void OnComputerScanCompleted(Object sender, ComputerScanCompletedEventArgs e)
        {

            if (e.CompletedResult.SafeFilesCount != e.CompletedResult.TotalFilesCount)
            {
                var scanner = sender as IAptScanner;
                if (scanner != null) scanner.ComputerScanCompleted -= OnComputerScanCompleted;
            }
        }

        private void StartValkyrieAnalysis()
        {
            lock (this)
            {
                if (_isValkyrieStarted) return;
                _isValkyrieStarted = true;
            }
            string camLicenseId;
            try
            {
                camLicenseId = _camLicense ??
                               (_camLicense = _camLicenseProvider.TryGetFreeLicense(_proxyCredentials));
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
                return;
            }
            _manager.StartValkyrieAnalysis(_systemLogin, _systemPassword, camLicenseId, null,
                null, _proxyCredentials, Settings.Default.applicationName);
        }

        public void OnErrorOccurred(Exception ex)
        {
            var webException = ex as WebException;
            var httpWebResponse = webException?.Response as HttpWebResponse;
            HttpStatusCode? code = httpWebResponse?.StatusCode;
            if (code == HttpStatusCode.ProxyAuthenticationRequired)
            {
                _proxyCredentials = _proxyCredentialsProvider.GetProxyCredentials();
                if (_proxyCredentials != null)
                {
                    _isValkyrieStarted = false;
                    StartValkyrieAnalysis();
                }
                else
                    StopScan(new Guid[0]);
                return;
            }
            Action a = () =>
            {
                FatScanCompleted?.Invoke(this, new ErrorEventArgs(ex));
            };
            App.Current.Dispatcher.BeginInvoke(a);
            StopScan(new Guid[0]);
        }

        public GetModifiedResultsForOperationEventArgs GetModifiedResultsForOperation(Guid operationUid, TimeSpan? lastUtcTime,
            DateTime? lastFlcUtcTime, DateTime? lastValkyrieTime)
        {
            var result = _manager.AggregatedScanResults;
            var completed = !result.IsOperationInProgress(operationUid);
            List<IScanResultItem> mappedResult = new List<IScanResultItem>();
            Dictionary<string,IModule> modules = new Dictionary<string,IModule>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in result.ScanResults.Where(c => lastUtcTime == null || c.Modified > lastUtcTime))
            {
                var modified = r.Modified;
                var scanStage = r.ScanStage;
                List<IBadFileItem> badfiles = new List<IBadFileItem>();
                {
                    foreach (var m in r.BadFiles)
                    {
                        if (!modules.ContainsKey(m.Sha1) && (m.IsUnknown() || m.Cloud == CloudVerdict.Malware))
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
                        ScanStage = FromAptScanStage(scanStage),
                        Modified = modified,
                        StatusMessage = r.ShortStatusMessage.ToString(),
                        BadFiles = badfiles.ToArray()
                    });

                }
            }

            var files = _manager.GetModifiedFileVerdicts(lastFlcUtcTime);
            Dictionary<string, IValkyrieAnalysisResult> valkyrieAnalysisResults;
            var valkyrieResult = _manager.GetModifiedValkyrieVerdicts(lastValkyrieTime, modules, out valkyrieAnalysisResults);
            List<IFileVerdictContext> mappedFiles = files
                .Select(r => new FileVerdictContext(r.Sha1, r.Cloud, r.ModifiedDateTime)).OfType<IFileVerdictContext>().ToList();
            if (valkyrieResult.LastModifiedDate.HasValue)
                mappedFiles.AddRange(valkyrieResult.Results.Select(vr =>
                    new FileVerdictContext(vr.Sha1, vr, valkyrieResult.LastModifiedDate.Value)));
            mappedFiles.AddRange(valkyrieAnalysisResults.Values.Select(vr =>
                new FileVerdictContext(vr.Sha1, vr, DateTime.MinValue)));
            return new GetModifiedResultsForOperationEventArgs(mappedResult.ToArray(), mappedFiles.ToArray(), completed);
        }
        

        void OnValkyrieAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs e)
        {
            Action action;
            if (e.Error != null)
            {
                action = () =>
                {
                    OnErrorOccurred(e.Error);
                };
            }
            else
            {
                action = () =>
                {
                    ErrorEventArgs args = new ErrorEventArgs(null);
                    if (e.Cancelled)
                    {
                        args.Error = new OperationCanceledException("Scan operation was canceled.");
                    }
                    FatScanCompleted?.Invoke(this, args);
                };
            }

            App.Current.Dispatcher.BeginInvoke(action);
        }
        private void OnScanStarted(Object sender, ScanStartedEventArgs e)
        {
            var scanner = sender as IAptScanner;
            if (scanner != null)
            {
                scanner.ScanStarted -= OnScanStarted;
            }

            if (e.Error != null)
                return;

            StartValkyrieAnalysis();
            
            Action action = () =>
            {
                ScanStartedEventArgs args = new ScanStartedEventArgs
                {
                    OperationUid = e.OperationUid,
                    //RescanAttempt = e.RescanAttempt,
                    //Error = e.Error
                };
                FatScanStarted?.Invoke(this, args);
            };
            App.Current.Dispatcher.BeginInvoke(action);
        }
        public void StopScan(Guid[] targets)
        {
            _manager.StopScan(targets);

            CancelScanStarted?.Invoke(this, new EventArgs());
        }

        public void UpdateFlsPorts(int udpPort, int tcpPort)
        {
            _manager.UpdateFlsPorts(udpPort, tcpPort);
        }

        public async Task OnAppClosing()
        {
            await _manager.OnAppClosing();
        }

        public string GetValkyrieDetailsLink()
        {
            return _manager.GetValkyrieDetailsLink();
        }

        public string GetValkyrieDetailsLink(string apiKey, string scanId)
        {
            return _manager.GetValkyrieDetailsLink(apiKey, scanId);
        }

        public IOperationResult SubmitEmail(string[] addresses)
        {
            lock (this){
                if (_isValkyrieStarted == false)
                    StartValkyrieAnalysis();
            }
            var result = _manager.SubmitEmail(addresses);
            return result;
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

                case AptScanStage.RemoteOperationCompleted:
                    return ScanStage.RemoteOperationCompleted;

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
                _manager.Dispose();
                _manager = null;
            }
        }
        #endregion
    }
}
