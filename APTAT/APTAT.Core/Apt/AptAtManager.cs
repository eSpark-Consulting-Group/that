using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.Core.Descent.Apt;
using APTAT.Core.FLS;
using APTAT.Core.Properties;
using APTAT.Core.Reports;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Valkyrie;
using Valkyrie.WebApiProvider.Enums;
using ValkyrieVerdict = APTAT.Valkyrie.ValkyrieVerdict;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;

namespace APTAT.Core.Apt
{
    public class AptAtManager : IAptAtManagerInternal
    {
        private readonly IDescentSubSystem _descentSubSystem;
        private readonly IAptScannerFactory _aptScannerFactory;
        private readonly IFlsManager _flsManager;
        private IValkyrieClient _valkyrieClient;
        private readonly Dictionary<string, IAptVerdictContext> _flsHashResults 
            = new Dictionary<string, IAptVerdictContext>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<IAptVerdictContext> _verdictUpdateHistory 
            = new HashSet<IAptVerdictContext>();
        private readonly IDistinctFileHelper _distinctHelper;

        private bool _isValkyrieStarted;
        private bool _isValkyrieScanEnded = false;
        private DateTime? _latestValkyrieVerdictUpdate = null;
        
        private int _filesShouldSendToValkyrieCount = 0;
        private int _filesWithFlsErrorCount = 0;

        public event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<FileResult>> ValkyrieAnalyzeProgressChanged;
        public event EventHandler<FlsRequestEventArgs<IAptVerdictContext>> FlsAnalyzeProgressChanged;
        public event AnalyzeCompletedEventHandler<FileResult> ValkyrieAnalyzeCompleted;
        public event EventHandler ValkyrieAnalyzeStarted;
        public IAggregatedAptScanResult AggregatedScanResults { get; private set; }
        public Guid ValkyrieOperationUid { get; private set; }
        public Guid FlsLookupOperationUid { get; private set; }

        public Dictionary<string, FileResult> ValkyrieAnalysisResult { get; } =
            new Dictionary<string, FileResult>(StringComparer.OrdinalIgnoreCase);


        private IEnumerable<IAptScanResult> GetResultsEnumerator()
        {
            if(AggregatedScanResults == null) return new IAptScanResult[] {};
            return AggregatedScanResults.ScanResults;
        }

        private void UpdateLastResult(Dictionary<string, CloudVerdict> resultsWithoutErrors)
        {

            bool isUpdated = false;
            foreach (var result in GetResultsEnumerator().ToArray())
            {

                var files =
                    result.Files.Where(f => f.Cloud == CloudVerdict.FailedToAnalyze || f.Cloud == CloudVerdict.NoVerdict).ToArray();

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
                FlsLookupOperationUid = Guid.NewGuid();
            }
        }


        public AptAtManager(IDescentSubSystem descentSubSystem, IValkyrieClient valkyrieClient, IFlsManager manager,
            IAptScannerFactory aptScannerFactory, IDistinctFileHelper distinctHelper)
        {
            _descentSubSystem = descentSubSystem;
            _aptScannerFactory = aptScannerFactory;
            _distinctHelper = distinctHelper;

            _valkyrieClient = valkyrieClient;
            _valkyrieClient.SetBaseUri(new Uri(Settings.Default.valkyrieBaseUrl));
            _valkyrieClient.AnalyzeProgressChanged += OnValkyrieClientProgressChanged;
            _valkyrieClient.AnalyzeCompleted += OnValkyrieManagerAnalyzeCompleted;
            _valkyrieClient.AnalyzeStarted += OnValkyrieAnalyzeStarted;
            _flsManager = manager;
            _flsManager.FlsAnalyzed += FlsManagerOnAnalyzed;
        }


        private void OnValkyrieAnalyzeStarted(object sender, EventArgs e)
        {
            _isValkyrieStarted = true;
            _isValkyrieScanEnded = false;
            _filesShouldSendToValkyrieCount = 0;
            ValkyrieAnalyzeStarted?.Invoke(this, EventArgs.Empty);
        }

        private void OnValkyrieManagerAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<ValkyrieAnalysisResult> e)
        {
            var valkyrieAnalysisResult = GetFullResult(e.Result, GetResultsEnumerator().ToArray()).Values;
            var arg = new AnalyzeCompletedEventArgs<FileResult>(valkyrieAnalysisResult.ToArray(), e.Error, e.Cancelled
                , e.UserState);
            ValkyrieAnalyzeCompleted?.Invoke(this, arg);
        }

        private void OnValkyrieClientProgressChanged(object sender, ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieAnalysisResult> e)
        {
            ProgressInfo<ValkyrieAnalysisResult> info = e.ProgressInfo;

            var valkyrieAnalysisResult = GetFullResult(info.Results, GetResultsEnumerator().ToArray()).Values;
            var valkyrieAnalysisResultInternal = ValkyrieAnalysisResult;
            lock (ValkyrieAnalysisResult)
            {
                foreach (var fileResult in valkyrieAnalysisResult)
                {
                    FileResult file;
                    if (valkyrieAnalysisResultInternal.TryGetValue(fileResult.Sha1, out file))
                    {
                        valkyrieAnalysisResultInternal.Remove(file.Sha1);
                        if (!e.IsUpdate)
                            Debug.Assert(true, "uploaded twice the same file to valkyrie");
                        if (file.ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.InQueue)
                        {
                            if (_filesShouldSendToValkyrieCount > 0)
                                _filesShouldSendToValkyrieCount--;
                        }
                    }
                    if (file != null || !e.IsUpdate)
                    {
                        valkyrieAnalysisResultInternal.Add(fileResult.Sha1, fileResult);

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
                    if (!AggregatedScanResults.IsOperationInProgress() && _filesWithFlsErrorCount == 0 &&
                        _filesShouldSendToValkyrieCount == 0 && _isValkyrieStarted && !_isValkyrieScanEnded)
                    {
                        _isValkyrieScanEnded = true;
                        _valkyrieClient.SendEndScanAsync();
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
                        found = new FileResult(badFile);
                        var vResult =
                            valkyrieAnalysisResults.FirstOrDefault(v => v.SHA1 != null
                            && String.Equals(v.SHA1, badFile.Sha1, StringComparison.OrdinalIgnoreCase));

                        if (vResult != null)
                        {
                            found.SetValkyrieResult(vResult);
                            fileResults.Add(found.Sha1, found);
                        }
                    }

                    if (scanResult.AptReport != null && scanResult.AptReport.Computer != null)
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

        public IAptScanner GetAptScanner(IComputer[] targets, IDevicesSourceDescription devicesSourceDescription, ScanOptions options)
        {
            _flsManager.UpdateFlsPorts(int.Parse(options.UdpPort), int.Parse(options.TcpPort));
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
                    UpdateLastResult(dict);

                }
                if ((verdictUpdatedAny || newUnknownFiles.Any()) && _isValkyrieStarted)
                {
                    foreach (var unknownFile in newUnknownFiles)
                    {
                        if(!dictToValkyrie.ContainsKey(unknownFile.Sha1))
                            dictToValkyrie.Add(unknownFile.Sha1, unknownFile.Cloud);
                    }
                    List<ValkyrieFileInfo> filesToAnalyze = GetFilesToAnalyzeForValkyrie(dictToValkyrie);
                    if (filesToAnalyze.Any())
                    {
                        var valkyrieAnalysisResults = GetFullResult(filesToAnalyze.Select(v =>
                                    new ValkyrieAnalysisResult(v.Sha1, ValkyrieVerdict.InQueue)),
                                GetResultsEnumerator().ToArray()).Values;
                       _valkyrieClient.AddFilesToAnalyze(filesToAnalyze, fromRemoteScanner);
                        lock (ValkyrieAnalysisResult)
                        {
                            _filesShouldSendToValkyrieCount += filesToAnalyze.Count;
                            foreach (var fileInfo in valkyrieAnalysisResults)
                                ValkyrieAnalysisResult.Add(fileInfo.Sha1, fileInfo);
                        }
                        ValkyrieOperationUid = Guid.NewGuid();
                    }
                }
            }
            if (verdictUpdatedAny)
                OnFlsAnalyzeProgressChangedAsync(
                    new FlsRequestEventArgs<IAptVerdictContext>(verdictUpdated.ToArray(), null));
            if (addToAnalyze.Any())
                _flsManager.Enqueue(addToAnalyze.Select(f => f.Sha1), true);
            //  temp.Select(t)
            TryInvokeEndScan();
            return resultsUpdated;
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
                fileResults = ValkyrieAnalysisResult.Values.ToArray();
            }
            List<FileResult> result = new List<FileResult>();
            foreach (var fileResult in fileResults)
            {
                if (fileResult.VerdictModified > modifiedDate)
                    result.Add(fileResult);
                if(modules.ContainsKey(fileResult.Sha1))
                    valkyrieAnalysisResults.Add(fileResult.Sha1, fileResult.ValkyrieAnalysisResult);
            }
            return modifiedDate == null ? fileResults : result.ToArray();
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
            if (scanner == null) return;
            if (e.RescanAttempt && AggregatedScanResults != null)
                AggregatedScanResults.MergeOperationContext(scanner.ScanOperationContext);
            else
            {
                AggregatedScanResults = new AggregatedAptScanResult(scanner.ScanOperationContext);
                _flsManager.Cancel();
                _isValkyrieStarted = false;
                _filesShouldSendToValkyrieCount = 0;
                _filesWithFlsErrorCount = 0;
                _valkyrieClient.AnalyzeAsyncCancel();
                lock (_flsHashResults)
                {
                    _flsHashResults.Clear();
                    _verdictUpdateHistory.Clear();
                }
                ValkyrieAnalysisResult.Clear();
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
            TryInvokeEndScan();
            if (_isValkyrieStarted )
            {
                IAptScanResult[] aptScanResults = GetResultsEnumerator().ToArray();
                this._valkyrieClient.AllowSubmit(_distinctHelper.GetDistinctFileInfoDict(aptScanResults));
            }
        }

        public IAptReportBuilder GetAptReportBuilder()
        {
            IAptReportBuilder aptReportBuilder = new AptReportBuilder(this);
            return aptReportBuilder;
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
                List<ValkyrieFileInfo> distinctFileList = GetDistinctUknownFileInfo(aptScanResults);
                _valkyrieClient.StartAnalyze(camRegistration, distinctFileList, login, password, aptScanResults.Length,
                    proxyCredential, productName);
            }
        }

        public string GetValkyrieDetailsLink()
        {
            return _valkyrieClient.GetValkyrieDetailsLink();
        }

        public OperationResult SubmitEmail(string[] addresses)
        {
            var response = _valkyrieClient.SubmitEmail(addresses);

            if (response.Error == null && response.ReturnCode == ErrorCodesEnum.Success)
            {
                return new OperationResult() {IsSucceeded = true};
            }
            return new OperationResult() { IsSucceeded = false, Error = response.Error, ErrorMessage = response.ResultMessage};
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
            }
        }
        public async Task OnAppClosing()
        {
            if (_isValkyrieScanEnded) return;
            await _valkyrieClient.SendEndScan();
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
    }

}
