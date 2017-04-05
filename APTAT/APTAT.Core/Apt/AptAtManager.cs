using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using APTAT.Core.Descent.Apt;
using APTAT.Core.FLS;
using APTAT.Core.Properties;
using APTAT.Core.Reports;
using APTAT.Core.Storage;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.Valkyrie;
using Valkyrie.WebApiProvider.Enums;
using ValkyrieVerdict = APTAT.Valkyrie.ValkyrieVerdict;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;
using ValkyrieFileInfo = APTAT.Valkyrie.ValkyrieFileInfo;
using APTAT.DAL.Enums;
using APTAT.Core.Valkyrie;
using System.Security.Cryptography;
using System.Text;

namespace APTAT.Core.Apt
{
    public class AptAtManager : IAptAtManager
    {
        private readonly IDescentSubSystem _descentSubSystem;
        private readonly IAptScannerFactory _aptScannerFactory;
        private readonly IFlsManager _flsManager;
        private readonly IStorageSubSystem _entityStorage;
        private IValkyrieClientEx _valkyrieClient;
        private readonly Dictionary<string, IAptVerdictContext> _flsHashResults
            = new Dictionary<string, IAptVerdictContext>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<IAptVerdictContext> _verdictUpdateHistory
            = new HashSet<IAptVerdictContext>();
        private readonly IDistinctFileHelper _distinctHelper;

        readonly Dictionary<string, CloudVerdict> _queueToValkyrie
            = new Dictionary<string, CloudVerdict>(StringComparer.OrdinalIgnoreCase);

        private bool _valkyrieScanStarted;
        private bool? _isValkyrieScanEnded = false;
        private DateTime? _latestValkyrieVerdictUpdate = null;

        private int _filesShouldSendToValkyrieCount = 0;
        private int _filesWithFlsErrorCount = 0;
        
        
        private readonly IReportBuilderFactory _reportBuilderFactory;

        public event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<FileResult>> ValkyrieAnalyzeProgressChanged;
        public event EventHandler<FlsRequestEventArgs<IAptVerdictContext>> FlsAnalyzeProgressChanged;
        public event EventHandler<AnalyzeCompletedEventArgs> ValkyrieAnalyzeCompleted;
        public event EventHandler ValkyrieAnalyzeStarted;
        public IAggregatedAptScanResult AggregatedScanResults { get; private set; }
        public Guid ValkyrieOperationUid { get; private set; }
        public Guid FlsLookupOperationUid { get; private set; }

        public Dictionary<string, ValkyrieAnalysisResult> ValkyrieAnalysisResult { get; } =
            new Dictionary<string, ValkyrieAnalysisResult>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, FileResult> _valkyrieAnalysisResult { get; } =
            new Dictionary<string, FileResult>(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> ShouldSendToValkyrie { get; } =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private IEnumerable<IAptScanResult> GetResultsEnumerator()
        {
            if (AggregatedScanResults == null) return new IAptScanResult[] { };
            return AggregatedScanResults.ScanResults;
        }

        private void UpdateLastResult(Dictionary<string, CloudVerdict> resultsWithoutErrors, VerdictSource verdictSource)
        {
            bool isUpdated = false;
            foreach (var result in GetResultsEnumerator().ToArray())
            {
                var files =
                    result.Files.Where(f => f.Cloud != CloudVerdict.Safe && f.Cloud != CloudVerdict.Malware).ToArray();

                foreach (var file in files)
                {
                    if (resultsWithoutErrors.ContainsKey(file.Sha1))
                    {
                        file.Cloud = resultsWithoutErrors[file.Sha1];
                        isUpdated = true;
                    }
                }
                result.RecalculateStatistics();
            }
            if (isUpdated)
            {
                _entityStorage.UpdateVerdictForOperation(AggregatedScanResults.OperationUid, resultsWithoutErrors, verdictSource);
                FlsLookupOperationUid = Guid.NewGuid();
            }
        }


        public AptAtManager(IDescentSubSystem descentSubSystem, IValkyrieClientEx valkyrieClient, IFlsManager manager,
            IAptScannerFactory aptScannerFactory, IDistinctFileHelper distinctHelper, IStorageSubSystem storage, 
            IReportBuilderFactory reportBuilderFactory)
        {
            _descentSubSystem = descentSubSystem;
            _entityStorage = storage;
            _aptScannerFactory = aptScannerFactory;
            _distinctHelper = distinctHelper;
            _reportBuilderFactory = reportBuilderFactory;           

            _valkyrieClient = valkyrieClient;
            _valkyrieClient.SetBaseUri(new Uri(Settings.Default.valkyrieBaseUrl));
            _valkyrieClient.AnalyzeProgressChanged += OnValkyrieClientProgressChanged;
            _valkyrieClient.AnalyzeCompleted += OnValkyrieManagerAnalyzeCompleted;
            _valkyrieClient.AnalyzeStarted += OnValkyrieAnalyzeStarted;
            _flsManager = manager;
            _flsManager.FlsAnalyzed += FlsManagerOnAnalyzed;
        }


        private void OnValkyrieAnalyzeStarted(object sender, ValkyrieAnalyzeStartedEventArgs e)
        {
            _isValkyrieScanEnded = null;
            _filesShouldSendToValkyrieCount = 0;
            ValkyrieAnalysisResult.Clear();
            ShouldSendToValkyrie.Clear();
            ValkyrieOperationUid = Guid.NewGuid();
            ValkyrieAnalyzeStarted?.Invoke(this, EventArgs.Empty);
            lock (_queueToValkyrie)
            {
                _valkyrieScanStarted = true;
                if (_queueToValkyrie.Count > 0)
                    AddFilesToValkyrieAnalysis(true, _queueToValkyrie);
                _queueToValkyrie.Clear();
            }
        }

        private void OnValkyrieManagerAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs e)
        {
            var arg = new AnalyzeCompletedEventArgs(e.Error, e.Cancelled, e.UserState);
            ValkyrieAnalyzeCompleted?.Invoke(this, arg);
        }

        private void OnValkyrieClientProgressChanged(object sender, ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieAnalysisResult> e)
        {
            ProgressInfo<ValkyrieAnalysisResult> info = e.ProgressInfo;

            var valkyrieAnalysisResultDict = GetFullResult(info.Results, GetResultsEnumerator().ToArray());
            var valkyrieAnalysisResult = valkyrieAnalysisResultDict.Values;
            var valkyrieAnalysisResultInternal = _valkyrieAnalysisResult;
            lock (ValkyrieAnalysisResult)
            {
                foreach (var fileResult in valkyrieAnalysisResult)
                {
                    FileResult file;
                    if (valkyrieAnalysisResultInternal.TryGetValue(fileResult.Sha1, out file))
                    {
                        valkyrieAnalysisResultInternal.Remove(file.Sha1);
                        ValkyrieAnalysisResult.Remove(fileResult.Sha1);
                        if (!e.IsUpdate)
                            Debug.Assert(true, "uploaded twice the same file to valkyrie");
                        if (file.ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.InQueue)
                        {
                            if (_filesShouldSendToValkyrieCount > 0)
                                _filesShouldSendToValkyrieCount--;
                            ShouldSendToValkyrie.Remove(file.Sha1);
                        }
                    }
                    if (file != null || !e.IsUpdate)
                    {
                        valkyrieAnalysisResultInternal.Add(fileResult.Sha1, fileResult);
                        ValkyrieAnalysisResult.Add(fileResult.Sha1, fileResult.ValkyrieAnalysisResult);

                        if (_latestValkyrieVerdictUpdate == null || fileResult.VerdictModified > _latestValkyrieVerdictUpdate.Value)
                            _latestValkyrieVerdictUpdate = fileResult.VerdictModified;
                    }
                }
                if (valkyrieAnalysisResult.Any())
                    ValkyrieOperationUid = Guid.NewGuid();
            }
            ProgressInfo<FileResult> progressInfo = new ProgressInfo<FileResult>(info.TotalFileCount,
                info.ProcessedFileCount, valkyrieAnalysisResult.ToArray());
            ValkyrieAnalyzeProgressChanged?.Invoke(this,
                                   new ValkyrieAnalyzeProgressChangedEventArgs<FileResult>(e.Percent, progressInfo, e.IsUpdate));
            TryInvokeEndScan();
        }

        private void TryInvokeEndScan()
        {
            lock (_flsHashResults)
            {
                lock (ValkyrieAnalysisResult)
                {
                    if (AggregatedScanResults != null && !AggregatedScanResults.IsOperationInProgress() 
                        && _valkyrieScanStarted && !_isValkyrieScanEnded.GetValueOrDefault())
                    {
                        if (ShouldSendToValkyrie.Count == 0 && _isValkyrieScanEnded == null)
                        {
                            _isValkyrieScanEnded = false;
                            _valkyrieClient.TryFinishUsersScanOnStorage();
                        }
                        if (_filesWithFlsErrorCount == 0 && _filesShouldSendToValkyrieCount == 0)
                        {
                            _isValkyrieScanEnded = true;
                            _valkyrieClient.SendEndScanAsync();
                        }
                    }
                }
            }
        }

        private static Dictionary<string, FileResult> GetFullResult(IEnumerable<ValkyrieAnalysisResult> valkyrieResult,
            IEnumerable<IAptScanResult> scanResults)
        {
            Dictionary<string, FileResult> fileResults = new Dictionary<string, FileResult>(StringComparer.OrdinalIgnoreCase);
            var valkyrieAnalysisResults = valkyrieResult as ValkyrieAnalysisResult[] ?? valkyrieResult.ToArray();
            foreach (var scanResult in scanResults)
            {
                foreach (var badFile in scanResult.BadFiles)
                {
                    FileResult found;
                    if (!fileResults.TryGetValue(badFile.Sha1, out found))
                    {
                        var vResult =
                            valkyrieAnalysisResults.FirstOrDefault(v => 
                            String.Equals(v.SHA1, badFile.Sha1, StringComparison.OrdinalIgnoreCase));

                        if (vResult != null)
                        {
                            found = new FileResult(badFile, vResult);
                            fileResults.Add(found.Sha1, found);
                        }
                    }

                    if (found != null && scanResult.AptReport != null 
                        && scanResult.AptReport.Computer != null)
                    {
                        var computer = scanResult.AptReport.Computer;
                        var comp = new ComputerInformation(computer, scanResult.TargetUid);
                        if (!found.Computers.Any(c => c.IP == comp.IP && c.FilePath == comp.FilePath))
                        {
                            comp.FilePath = badFile.Path;
                            found.Computers.Add(comp);
                        }
                    }
                }
            }
            return fileResults;
        }

        private List<ValkyrieFileInfo> GetDistinctUknownFileInfo(IAptScanResult[] results)
        {
            return _distinctHelper.GetDistinctFileInfo(results);
        }

        public IAptScanner GetAptScanner(IComputer[] targets, IDevicesSourceDescription devicesSourceDescription, IScanOptions options)
        {
            _flsManager.UpdateFlsPorts(options.UdpPort, options.TcpPort);
            var scanner = _aptScannerFactory
                .CreateAptScanner(_descentSubSystem, targets, devicesSourceDescription, options);
            scanner.ScanStarted += OnScanStarted;
            scanner.ComputerScanCompleted += OnScannerOnScanCompleted;
            scanner.ScanCompleted += OnScanCompleted;
            return scanner;
        }


        private void OnScannerOnScanCompleted(object sender, ComputerScanCompletedEventArgs args)
        {
            var resultsUpdated = AddResultsToDictionary(args.CompletedResult.Files.ToArray(), true);
            if (resultsUpdated)
                args.CompletedResult.RecalculateStatistics();
            _entityStorage.SaveScanResults(args.OperationUid, args.CompletedResult);
        }

        private bool AddResultsToDictionary(IFlsResult[] resultsFiles, bool fromRemoteScanner = false)
        {
            var resultsUpdated = false;
            var verdictUpdatedAny = false;
            var addToAnalyze = new HashSet<IFlsResult>();
            var verdictUpdated = new List<IAptVerdictContext>();
            var newUnknownFiles = new List<IFlsResult>();
            lock (_flsHashResults)
            {
                if (!fromRemoteScanner)
                    _filesWithFlsErrorCount -= _filesWithFlsErrorCount;
                foreach (var r in resultsFiles)
                {
                    if (!_flsHashResults.ContainsKey(r.Sha1))
                    {
                        var verdictContext = new AptVerdictContext(r.Sha1, r.Cloud);
                        _flsHashResults[r.Sha1] = verdictContext;
                        if (_distinctHelper.ShouldSendToValkyrie(verdictContext))
                            newUnknownFiles.Add(verdictContext);
                        if (r.IsNotAnalyzed())
                            addToAnalyze.Add(verdictContext);
                    }
                    else
                    {
                        var item = _flsHashResults[r.Sha1];
                        if (r.IsNotAnalyzed())
                        {
                            if (item.Cloud != CloudVerdict.FailedToAnalyze &&
                                item.Cloud != CloudVerdict.NoVerdict)
                            {
                                r.Cloud = item.Cloud;
                                resultsUpdated = true;
                            }
                        }
                        else if (item.Cloud != r.Cloud && item.IsNotAnalyzed())
                        {
                            item.Cloud = r.Cloud;
                            var clone = item.Clone();
                            verdictUpdated.Add(clone);
                            addToAnalyze.Remove(item);
                        }
                    }
                }
                _filesWithFlsErrorCount += addToAnalyze.Count;
                verdictUpdatedAny = verdictUpdated.Any();
                var dictToValkyrie = new Dictionary<string, CloudVerdict>(StringComparer.OrdinalIgnoreCase);
                if (verdictUpdatedAny)
                {
                    var dict = new Dictionary<string, CloudVerdict>(StringComparer.OrdinalIgnoreCase);
                    lock (_verdictUpdateHistory)
                        foreach (var result in verdictUpdated)
                        {
                            _verdictUpdateHistory.Remove(result);
                            _verdictUpdateHistory.Add(result);
                            dict.Add(result.Sha1, result.Cloud);
                            if (_distinctHelper.ShouldSendToValkyrie(result))
                                dictToValkyrie.Add(result.Sha1, result.Cloud);
                        }
                    UpdateLastResult(dict, fromRemoteScanner? VerdictSource.RemoteAgent : VerdictSource.Apt);

                }
                if ((dictToValkyrie.Any() || newUnknownFiles.Any()))
                {
                    foreach (var unknownFile in newUnknownFiles)
                    {
                        if (!dictToValkyrie.ContainsKey(unknownFile.Sha1))
                            dictToValkyrie.Add(unknownFile.Sha1, unknownFile.Cloud);
                    }
                    lock(_queueToValkyrie)
                    {
                        if (_valkyrieScanStarted)
                        {
                            AddFilesToValkyrieAnalysis(fromRemoteScanner, dictToValkyrie);
                        }
                        else
                            foreach (var value in dictToValkyrie)
                            {
                                _queueToValkyrie.Add(value.Key, value.Value);
                            }
                    }
                }
            }
            if (verdictUpdatedAny)
                OnFlsAnalyzeProgressChangedAsync(
                    new FlsRequestEventArgs<IAptVerdictContext>(verdictUpdated.ToArray(), null));
            if (addToAnalyze.Any())
                _flsManager.Enqueue(addToAnalyze.Select(f => f.Sha1));
            //  temp.Select(t)
            TryInvokeEndScan();
            return resultsUpdated;
        }

        private void AddFilesToValkyrieAnalysis(bool fromRemoteScanner, Dictionary<string, CloudVerdict> dictToValkyrie)
        {
            List<ValkyrieFileInfo> filesToAnalyze = GetFilesToAnalyzeForValkyrie(dictToValkyrie);
            if (filesToAnalyze.Any())
            {
                var valkyrieAnalysisResults = GetFullResult(filesToAnalyze.Select(v =>
                            new ValkyrieAnalysisResult(v.Sha1, ValkyrieVerdict.InQueue)),
                        GetResultsEnumerator().ToArray()).Values;
                lock (ValkyrieAnalysisResult)
                {
                    _filesShouldSendToValkyrieCount += filesToAnalyze.Count;
                    foreach (var fileInfo in valkyrieAnalysisResults)
                    {
                        _valkyrieAnalysisResult.Add(fileInfo.Sha1, fileInfo);
                        ValkyrieAnalysisResult.Add(fileInfo.Sha1, fileInfo.ValkyrieAnalysisResult);
                        if (fromRemoteScanner)
                            ShouldSendToValkyrie.Add(fileInfo.Sha1);
                    }
                }
                _valkyrieClient.AddFilesToAnalyze(filesToAnalyze, fromRemoteScanner);
                ValkyrieOperationUid = Guid.NewGuid();
            }
        }

        private List<ValkyrieFileInfo> GetFilesToAnalyzeForValkyrie(Dictionary<string, CloudVerdict> dict)
        {
            IAptScanResult[] aptScanResults = GetResultsEnumerator().ToArray();
            List<ValkyrieFileInfo> distinctFileList = GetDistinctUknownFileInfo(aptScanResults);
            List<ValkyrieFileInfo> fileInfoList = distinctFileList.Where(x => dict.ContainsKey(x.Sha1)).ToList();

            return fileInfoList;
        }

        public int GetDistinctFilesCount()
        {
            return AggregatedScanResults.GetDistinctFilesCount();
        }

        public IAptVerdictContext[] GetModifiedFileVerdicts(DateTime? modifiedDate)
        {
            IAptVerdictContext[] verdicts;
            lock (_verdictUpdateHistory)
            {
                verdicts = _verdictUpdateHistory.ToArray();
            }
            return modifiedDate == null ? verdicts
                : verdicts.Where(vc => vc.ModifiedDateTime > modifiedDate).ToArray();
        }

        public FileResult[] GetModifiedValkyrieVerdicts(DateTime? modifiedDate, Dictionary<string, IModule> modules
            , out Dictionary<string, ValkyrieAnalysisResult> valkyrieAnalysisResults)
        {
            valkyrieAnalysisResults = new Dictionary<string, ValkyrieAnalysisResult>(StringComparer.OrdinalIgnoreCase);
            if (modules.Count == 0 && modifiedDate.HasValue && modifiedDate >= _latestValkyrieVerdictUpdate)
            {
                return new FileResult[0];
            }
            FileResult[] fileResults;
            lock (ValkyrieAnalysisResult)
            {
                fileResults = _valkyrieAnalysisResult.Values.ToArray();
            }
            List<FileResult> result = new List<FileResult>();
            foreach (var fileResult in fileResults)
            {
                if (fileResult.VerdictModified > modifiedDate)
                    result.Add(fileResult);
                if (modules.ContainsKey(fileResult.Sha1))
                    valkyrieAnalysisResults.Add(fileResult.Sha1, fileResult.ValkyrieAnalysisResult);
            }
            return modifiedDate == null ? fileResults.ToArray() : result.ToArray();
        }

        private void FlsManagerOnAnalyzed(object sender, FlsRequestEventArgs<IFlsResult> e)
        {
            if (e.Results.Any())
            {
                AddResultsToDictionary(e.Results);
                //OnFlsAnalyzeProgressChanged(e);
            }
        }

        private void OnScanStarted(object sender, ScanStartedEventArgs e)
        {
            IAptScanner scanner = sender as IAptScanner;
            if (scanner == null || scanner.ScanOperationContext == null) return;
            /*if (e.RescanAttempt && AggregatedScanResults != null)
                AggregatedScanResults.MergeOperationContext(scanner.ScanOperationContext);
            else*/
            {
                AggregatedScanResults = new AggregatedAptScanResult(scanner.ScanOperationContext);
                _flsManager.Cancel();
                _filesShouldSendToValkyrieCount = 0;
                _filesWithFlsErrorCount = 0;
                if (ValkyrieOperationUid != Guid.Empty)
                {
                    _valkyrieClient.AnalyzeAsyncCancel();
                    ValkyrieOperationUid = Guid.NewGuid();
                }
                _valkyrieScanStarted = false;
                lock (_flsHashResults)
                {
                    _flsHashResults.Clear();
                    _verdictUpdateHistory.Clear();
                }
                lock(_queueToValkyrie)
                    _queueToValkyrie.Clear();
                ShouldSendToValkyrie.Clear();
                ValkyrieAnalysisResult.Clear();
                _entityStorage.SaveOperationContext(scanner.ScanOperationContext);
            }
        }

        private void OnScanCompleted(object sender, ScanCompletedEventArgs e)
        {
            IAptScanner scanner = sender as IAptScanner;
            if (scanner != null)
            {
                scanner.ScanStarted -= OnScanStarted;
                scanner.ComputerScanCompleted -= OnScannerOnScanCompleted;
                scanner.ScanCompleted -= OnScanCompleted;
            }
            if (e.SucceededCount > 0)
                FlsLookupOperationUid = Guid.NewGuid();
            _entityStorage.UpdateScanResults(e.OperationContext);
            TryInvokeEndScan();
            if (_valkyrieScanStarted)
            {
                IAptScanResult[] aptScanResults = GetResultsEnumerator().ToArray();
                _valkyrieClient.AllowSubmit(_distinctHelper.GetDistinctFileInfoDict(aptScanResults));
            }
        }
        
        public void StopScan(Guid[] targetsUids)
        {
            if (targetsUids.Any())
                _descentSubSystem.StopOperationForTargetUid(targetsUids);
            else
            {
                _descentSubSystem.StopAllScans(0);

                _flsManager.Cancel();
                _valkyrieClient.AnalyzeAsyncCancel();
                _isValkyrieScanEnded = true;
                    
                lock (_flsHashResults)
                {
                    _flsHashResults.Clear();
                    _verdictUpdateHistory.Clear();
                }
            }
        }
        
        public void StartValkyrieAnalysis(string login, string password,
            string camLicenseId, string camUserName, string camPassword, NetworkCredential proxyCredential
            , string productName)
        {
            lock (_flsHashResults)
            {
                var camRegistration = new CamRegistration(camLicenseId, camUserName, camPassword);
                var results = GetResultsEnumerator();
                var aptScanResults = results as IAptScanResult[] ?? results.ToArray();
                _valkyrieClient.StartAnalyze(camRegistration, login, password, aptScanResults.Length,
                    proxyCredential, productName, AggregatedScanResults.OperationUid);
            }
        }

        public string GetValkyrieDetailsLink()
        {
            return _valkyrieClient.GetValkyrieDetailsLink();
        }

        public string GetValkyrieDetailsLink(string apiKey, string scanId)
        {
            return _valkyrieClient.GetValkyrieDetailsLink(apiKey, scanId);
        }

        public OperationResult SubmitEmail(string[] addresses)
        {
            var response = _valkyrieClient.SubmitEmail(addresses);

            if (response.Error == null && response.ReturnCode == ErrorCodesEnum.Success)
            {
                return new OperationResult() { IsSucceeded = true };
            }
            return new OperationResult() { IsSucceeded = false, Error = response.Error, ErrorMessage = response.ResultMessage };
        }

        public void Dispose()
        {
            if (_valkyrieClient != null)
            {
                _valkyrieClient.AnalyzeProgressChanged -= OnValkyrieClientProgressChanged;
                _valkyrieClient.AnalyzeCompleted -= OnValkyrieManagerAnalyzeCompleted;
                _valkyrieClient.AnalyzeStarted -= OnValkyrieAnalyzeStarted;
                _valkyrieClient.Dispose();
                _valkyrieClient = null;
            }
            if (_flsManager != null)
            {
                _flsManager.FlsAnalyzed -= FlsManagerOnAnalyzed;
                _flsManager.Dispose();
            }
        }
        public async Task OnAppClosing()
        {
            if (_isValkyrieScanEnded.GetValueOrDefault()) return;
            if (_valkyrieScanStarted)
            {
                await _valkyrieClient.SendEndScan();
            }
        }

        public void UpdateFlsPorts(int udpPort, int tcpPort)
        {
            _flsManager.UpdateFlsPorts(udpPort, tcpPort);
        }

        protected virtual void OnFlsAnalyzeProgressChangedAsync(FlsRequestEventArgs<IAptVerdictContext> e)
        {
            Action action = () =>
            {
                FlsAnalyzeProgressChanged?.Invoke(this, e);
            };
            Task.Factory.StartNew(action, TaskCreationOptions.None);
        }

        public async Task<ReportFile> BuildReportAsync(ReportType reportType, bool saveFile = true)
        {
            var reportBuilder = _reportBuilderFactory.GetReportBuilder(reportType.ScanStage());
            return await reportBuilder.BuildReportAsync(GetReportData(), reportType, saveFile);
        }

        private ReportData GetReportData()
        {
            var operationContext = AggregatedScanResults;

            if (!operationContext.CompletedTime.HasValue || operationContext.CompletedTime.Value == DateTime.MinValue)
                throw new DataNotReadyException();

            ReportData reportData = new ReportData()
            {
                OperationUid = CurrentOperationUid,
                ValkyrieAnalysisResult = ValkyrieAnalysisResult,
                OperationContext = operationContext
            };

            return reportData;
        }

        public string CurrentOperationUid
        {
            get
            {
                var uid = AggregatedScanResults.OperationUid + FlsLookupOperationUid.ToString() + ValkyrieOperationUid;
                return Hash(uid);
            }
        }

        private static string Hash(string input)
        {
            var hash = (new SHA1Managed()).ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }
    }

}
