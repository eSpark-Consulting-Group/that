using System;
using System.Collections.Generic;
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
using Valkyrie.WebApiProvider.Enums;
using ValkyrieVerdict = APTAT.Core.Valkyrie.ValkyrieVerdict;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;
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

   
        private readonly IReportBuilderFactory _reportBuilderFactory;

        public event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<FileResult>> ValkyrieAnalyzeProgressChanged;
        public event EventHandler<FlsRequestEventArgs<IAptVerdictContext>> FlsAnalyzeProgressChanged;
        public event EventHandler<AnalyzeCompletedEventArgs> ValkyrieAnalyzeCompleted;
        public event EventHandler ValkyrieAnalyzeStarted;

        #region Valkyrie Storage
        public Guid ValkyrieOperationUid { get; private set; }

        public Dictionary<string, IValkyrieAnalysisResult> ValkyrieAnalysisResult { get; } =
            new Dictionary<string, IValkyrieAnalysisResult>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _shouldSendToValkyrie =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private int _filesShouldSendToValkyrieCount = 0;

        private readonly SortedDictionary<DateTime, List<IValkyrieAnalysisResult>> _valkyrieAnalysisResultsModified
            = new SortedDictionary<DateTime, List<IValkyrieAnalysisResult>>();

        private readonly Dictionary<string, DateTime> _valkyrieHashAndModifiedDateMapped 
            = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        
        private void AddValkyrieResults(IValkyrieAnalysisResult[] results, bool fromRemoteScanner)
        {
            lock (ValkyrieAnalysisResult)
            {
                var modifiedDate = DateTime.UtcNow;
                List<IValkyrieAnalysisResult> list;
                _valkyrieAnalysisResultsModified.TryGetValue(modifiedDate, out list);
                _filesShouldSendToValkyrieCount += results.Length;
                foreach (var result in results)
                {
                    if (list == null)
                    {
                        list = new List<IValkyrieAnalysisResult>(results.Length);
                        _valkyrieAnalysisResultsModified[modifiedDate] = list;
                    }
                    list.Add(result);
                    _valkyrieHashAndModifiedDateMapped[result.Sha1] = modifiedDate;
                    ValkyrieAnalysisResult.Add(result.Sha1, result);
                    if (fromRemoteScanner)
                        _shouldSendToValkyrie.Add(result.Sha1);
                }
                ValkyrieOperationUid = Guid.NewGuid();
            }

        }
        public void UpdateValkyrieResults(IValkyrieAnalysisResult[] results)
        {
            lock (ValkyrieAnalysisResult)
            {
                var modifiedDate = DateTime.UtcNow;
                List<IValkyrieAnalysisResult> list;
                _valkyrieAnalysisResultsModified.TryGetValue(modifiedDate, out list);
                foreach (var result in results)
                {
                    IValkyrieAnalysisResult oldResult;
                    if (ValkyrieAnalysisResult.TryGetValue(result.Sha1, out oldResult))
                    {
                        var oldDate = _valkyrieHashAndModifiedDateMapped[oldResult.Sha1];
                        _valkyrieHashAndModifiedDateMapped[oldResult.Sha1] = modifiedDate;

                        var oldList = _valkyrieAnalysisResultsModified[oldDate];
                        if (oldList.Count == 1)
                            _valkyrieAnalysisResultsModified.Remove(oldDate);
                        else
                            oldList.Remove(oldResult);
                        ValkyrieAnalysisResult.Remove(result.Sha1);

                        if (list == null)
                        {
                            list = new List<IValkyrieAnalysisResult>(results.Length);
                            _valkyrieAnalysisResultsModified[modifiedDate] = list;
                        }
                        list.Add(result);

                        if (oldResult.Verdict == ValkyrieVerdict.InQueue)
                        {
                            if (_filesShouldSendToValkyrieCount > 0)
                                _filesShouldSendToValkyrieCount--;
                            _shouldSendToValkyrie.Remove(oldResult.Sha1);
                        }
                        ValkyrieAnalysisResult.Add(result.Sha1, result);

                    }
                }
                if (results.Any())
                {
                    ValkyrieOperationUid = Guid.NewGuid();
                    if (_latestValkyrieVerdictUpdate == null || modifiedDate > _latestValkyrieVerdictUpdate.Value)
                        _latestValkyrieVerdictUpdate = modifiedDate;
                }
            }
        }

        public IValkyrieTrackableInfo GetModifiedValkyrieVerdicts(DateTime? modifiedDate,
            Dictionary<string, IModule> modules, out Dictionary<string, IValkyrieAnalysisResult> valkyrieAnalysisResults)
        {
            valkyrieAnalysisResults = new Dictionary<string, IValkyrieAnalysisResult>(StringComparer.OrdinalIgnoreCase);
            if (modules.Count == 0 && modifiedDate.HasValue && modifiedDate >= _latestValkyrieVerdictUpdate)
            {
                return new ValkyrieTrackableInfo(modifiedDate, new IValkyrieAnalysisResult[0]);
            }
            KeyValuePair<DateTime, List<IValkyrieAnalysisResult>>[] fileResults;
            DateTime? lastModified;
            lock (ValkyrieAnalysisResult)
            {
                lastModified = _latestValkyrieVerdictUpdate;
                fileResults = _valkyrieAnalysisResultsModified.Select(r =>
                    new KeyValuePair<DateTime, List<IValkyrieAnalysisResult>>(r.Key, new List<IValkyrieAnalysisResult>(r.Value))).ToArray();
            }
            if (lastModified == null)
                return new ValkyrieTrackableInfo(modifiedDate, new IValkyrieAnalysisResult[0]);
            var result = new List<IValkyrieAnalysisResult>();
            for (var index = fileResults.Length - 1; index >= 0; index--)
            {
                var fileResult = fileResults[index];
                if (modifiedDate == null || fileResult.Key > modifiedDate)
                    result.AddRange(fileResult.Value);
                else if (modules.Count == 0)
                    break;
                foreach (var valkyrieResult in fileResult.Value)
                {
                    if (modules.ContainsKey(valkyrieResult.Sha1))
                        valkyrieAnalysisResults.Add(valkyrieResult.Sha1, valkyrieResult);
                }
            }
            return new ValkyrieTrackableInfo(lastModified, result.ToArray());
        }
        #endregion

        #region Fls Storage
        private int _filesWithFlsErrorCount;
        public IAggregatedAptScanResult AggregatedScanResults { get; private set; }
        public Guid FlsLookupOperationUid { get; private set; }
        private IEnumerable<IAptScanResult> GetResultsEnumerator()
        {
            if (AggregatedScanResults == null) return new IAptScanResult[] { };
            return AggregatedScanResults.ScanResults;
        }

        private void UpdateLastResult(Dictionary<string, CloudVerdict> resultsWithoutErrors, VerdictSource verdictSource)
        {
            var isUpdated = false;
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
        #endregion




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
            _shouldSendToValkyrie.Clear();
            _valkyrieAnalysisResultsModified.Clear();
            ValkyrieOperationUid = Guid.NewGuid();
            ValkyrieAnalyzeStarted?.Invoke(this, EventArgs.Empty);
            lock (_queueToValkyrie)
            {
                _valkyrieScanStarted = true;
                if (_queueToValkyrie.Count > 0)
                    AddFilesToValkyrieAnalysis(true, _queueToValkyrie);
                _queueToValkyrie.Clear();
            }
            if (!AggregatedScanResults.IsOperationInProgress())
            {
                var aptScanResults = GetResultsEnumerator().ToArray();
                _valkyrieClient.AllowSubmit(_distinctHelper.GetDistinctFileInfoDict(aptScanResults));
            }
            TryInvokeEndScan();
        }

        private void OnValkyrieManagerAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs e)
        {
            var arg = new AnalyzeCompletedEventArgs(e.Error, e.Cancelled, e.UserState);
            ValkyrieAnalyzeCompleted?.Invoke(this, arg);
        }

        private void OnValkyrieClientProgressChanged(object sender, ValkyrieAnalyzeProgressChangedEventArgs<IValkyrieAnalysisResult> e)
        {
            var info = e.ProgressInfo;
            UpdateValkyrieResults(info.Results);
            var valkyrieAnalyzeProgressChanged = ValkyrieAnalyzeProgressChanged;
            if (valkyrieAnalyzeProgressChanged != null)
            {
                var valkyrieAnalysisResultDict = GetFullResult(info.Results, GetResultsEnumerator().ToArray());
                var valkyrieAnalysisResult = valkyrieAnalysisResultDict.Values;

                var progressInfo = new ProgressInfo<FileResult>(info.TotalFileCount,
                    info.ProcessedFileCount, valkyrieAnalysisResult.ToArray());

                valkyrieAnalyzeProgressChanged.Invoke(this, new ValkyrieAnalyzeProgressChangedEventArgs<FileResult>(e.Percent, progressInfo));
            }
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
                        if (_shouldSendToValkyrie.Count == 0 && _isValkyrieScanEnded == null)
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

        private static Dictionary<string, FileResult> GetFullResult(IEnumerable<IValkyrieAnalysisResult> valkyrieResult,
            IEnumerable<IAptScanResult> scanResults)
        {
            var fileResults = new Dictionary<string, FileResult>(StringComparer.OrdinalIgnoreCase);
            var valkyrieAnalysisResults = valkyrieResult as IValkyrieAnalysisResult[] ?? valkyrieResult.ToArray();
            foreach (var scanResult in scanResults)
            {
                foreach (var badFile in scanResult.BadFiles)
                {
                    if (!fileResults.TryGetValue(badFile.Sha1, out FileResult found))
                    {
                        var vResult =
                            valkyrieAnalysisResults.FirstOrDefault(v =>
                            String.Equals(v.Sha1, badFile.Sha1, StringComparison.OrdinalIgnoreCase));

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

        private List<IValkyrieFileInfo> GetDistinctUknownFileInfo(IAptScanResult[] results)
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
                    _filesWithFlsErrorCount -= resultsFiles.Length;
                foreach (var r in resultsFiles)
                {
                    if (!_flsHashResults.TryGetValue(r.Sha1, out IAptVerdictContext item))
                    {
                        var verdictContext = new AptVerdictContext(r.Sha1, r.Cloud);
                        _flsHashResults[r.Sha1] = verdictContext;
                        if (_distinctHelper.ShouldSendToValkyrie(verdictContext))
                            newUnknownFiles.Add(verdictContext);
                        if (r.IsNotAnalyzed())
                            addToAnalyze.Add(verdictContext);
                    }
                    else if (item.Cloud != r.Cloud)
                    {
                        if ((r.IsNotAnalyzed() || r.IsUnknown())
                            && !item.IsNotAnalyzed())
                        {
                            r.Cloud = item.Cloud;
                            resultsUpdated = true;
                        }
                        else if (item.IsNotAnalyzed() && !r.IsNotAnalyzed())
                        {
                            item.Cloud = r.Cloud;
                            var clone = item.Clone();
                            verdictUpdated.Add(clone);
                            addToAnalyze.Remove(item);
                        }
                        else if (item.IsUnknown())
                        {
                            item.Cloud = r.Cloud;
                            var clone = item.Clone();
                            verdictUpdated.Add(clone);
                            newUnknownFiles.Remove(item);
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
            var filesToAnalyze = GetFilesToAnalyzeForValkyrie(dictToValkyrie);
            if (filesToAnalyze.Any())
            {
                var valkyrieAnalysisResults = filesToAnalyze.Select(v =>
                            (IValkyrieAnalysisResult)new ValkyrieAnalysisResult(v.Sha1, ValkyrieVerdict.InQueue)).ToArray();
                AddValkyrieResults(valkyrieAnalysisResults, fromRemoteScanner);
                _valkyrieClient.AddFilesToAnalyze(filesToAnalyze, fromRemoteScanner);
            }
        }

        private List<IValkyrieFileInfo> GetFilesToAnalyzeForValkyrie(Dictionary<string, CloudVerdict> dict)
        {
            var aptScanResults = GetResultsEnumerator().ToArray();
            var distinctFileList = GetDistinctUknownFileInfo(aptScanResults);
            var fileInfoList = distinctFileList.Where(x => dict.ContainsKey(x.Sha1)).ToList();

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
            var scanner = sender as IAptScanner;
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
                _shouldSendToValkyrie.Clear();
                ValkyrieAnalysisResult.Clear();
                _entityStorage.SaveOperationContext(scanner.ScanOperationContext);
            }
        }

        private void OnScanCompleted(object sender, ScanCompletedEventArgs e)
        {
            var scanner = sender as IAptScanner;
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
                var aptScanResults = GetResultsEnumerator().ToArray();
                _valkyrieClient.AllowSubmit(_distinctHelper.GetDistinctFileInfoDict(aptScanResults));
            }
        }
        
        public void StopScan(Guid[] targetsUids)
        {
            if (targetsUids.Any())
                _descentSubSystem.StopOperationForTargetUid(targetsUids);
            else
            {
                _descentSubSystem.StopAllScans();

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

        public void InitEmail(string[] addresses)
        {
            _valkyrieClient.InitEmail(addresses);
        }

        public async Task<IOperationResult> SubmitEmailAsync(string[] addresses)
        {
            var response = await _valkyrieClient.SubmitEmailAsync(addresses);

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

            var reportData = new ReportData()
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
