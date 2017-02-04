using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using APTAT.Core.Descent.Apt;
using APTAT.Core.FLS;
using APTAT.Core.Properties;
using APTAT.Core.Reports;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Valkyrie;
using ValkyrieVerdict = APTAT.Valkyrie.ValkyrieVerdict;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;

namespace APTAT.Core.Apt
{
    public class AptAtManager : IAptAtManagerInternal
    {
        private readonly IDescentSubSystem _descentSubSystem;
        private readonly IAptScannerFactory _aptScannerFactory;
        private readonly IFlsManager _flsManager;
        public event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<FileResult>> ValkyrieAnalyzeProgressChanged;
        public event EventHandler<FlsRequestEventArgs<IAptVerdictContext>> FlsAnalyzeProgressChanged;
        public event AnalyzeCompletedEventHandler<FileResult> ValkyrieAnalyzeCompleted;
        public event EventHandler ValkyrieAnalyzeStarted;
        private IValkyrieClient _valkyrieClient;
        private readonly Dictionary<string, IAptVerdictContext> _flsHashResults 
            = new Dictionary<string, IAptVerdictContext>();
        private readonly HashSet<IAptVerdictContext> _verdictUpdateHistory 
            = new HashSet<IAptVerdictContext>();
        private readonly IDistinctFileHelper _distinctHelper;
        //private IAptScanOperationContext _lastAptResults;

        public IAggregatedAptScanResult AggregatedScanResults { get; private set; }
        public Guid ValkyrieOperationUid { get; private set; }
        public Guid FlsLookupOperationUid { get; private set; }

        public ConcurrentDictionary<string, FileResult> ValkyrieAnalysisResult { get; } =
            new ConcurrentDictionary<string, FileResult>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<FileResult> _valkyrieUpdateHistory
            = new HashSet<FileResult>();
        private IEnumerable<IAptScanResult> GetResultsEnumerator()
        {
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
            ValkyrieAnalyzeStarted?.Invoke(this, EventArgs.Empty);
        }

        private void OnValkyrieManagerAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<ValkyrieAnalysisResult> e)
        {
            var valkyrieAnalysisResult = GetFullResult(e.Result, GetResultsEnumerator().ToArray()).Values;
            foreach (var fileResult in valkyrieAnalysisResult)
                ValkyrieAnalysisResult.AddOrUpdate(fileResult.Sha1, fileResult, (s, result) => fileResult);
            ValkyrieOperationUid = Guid.NewGuid();
            var arg = new AnalyzeCompletedEventArgs<FileResult>(valkyrieAnalysisResult.ToArray(), e.Error, e.Cancelled
                , e.UserState);


            ValkyrieAnalyzeCompleted?.Invoke(this, arg);
        }

        private void OnValkyrieClientProgressChanged(object sender, ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieAnalysisResult> e)
        {
            var handler = ValkyrieAnalyzeProgressChanged;
            if (handler != null)
            {
                ProgressInfo<ValkyrieAnalysisResult> info = e.ProgressInfo;

                var valkyrieAnalysisResult = GetFullResult(info.Results, GetResultsEnumerator().ToArray()).Values;
                var valkyrieAnalysisResultInternal = ValkyrieAnalysisResult;
                if (e.IsUpdate)
                {
                    foreach (var fileResult in valkyrieAnalysisResult)
                    {
                        FileResult f;
                        valkyrieAnalysisResultInternal.TryRemove(fileResult.Sha1, out f);
                        valkyrieAnalysisResultInternal.TryAdd(fileResult.Sha1, fileResult);
                    }
                    if (valkyrieAnalysisResult.Any())
                        ValkyrieOperationUid = Guid.NewGuid();
                }
                else
                {
                    foreach (var fileResult in valkyrieAnalysisResult)
                        valkyrieAnalysisResultInternal.AddOrUpdate(fileResult.Sha1, fileResult,
                            (key, fr) => fileResult);
                }
                ProgressInfo<FileResult> progressInfo = new ProgressInfo<FileResult>(info.TotalFileCount,
                    info.ProcessedFileCount, valkyrieAnalysisResult.ToArray());
                handler(this,
                    new ValkyrieAnalyzeProgressChangedEventArgs<FileResult>(e.Percent, progressInfo, e.IsUpdate));
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
            return _distinctHelper.GetDistinctUknownFileInfo(results);
        }

        public IAptScanner GetAptScanner(DeploymentTarget[] targets, IDevicesSourceDescription devicesSourceDescription, ScanOptions options)
        {
            _flsManager.UpdateFlsPorts(int.Parse(options.UdpPort), int.Parse(options.TcpPort));
            var scanner = _aptScannerFactory
                .CreateAptScanner(_descentSubSystem, targets, devicesSourceDescription, options);
            scanner.ScanStarted += OnScanStarted;
            scanner.ComputerScanCompleted += OnScannerOnScanCompleted;
            scanner.ScanCompleted += OnScanCompleted;
            return scanner;
        }

        private bool _isValkyrieStarted;

        private void OnScannerOnScanCompleted(object sender, ComputerScanCompletedEventArgs args)
        {
            var resultsUpdated = AddResultsToDictionary(args.CompletedResult.Files.ToArray());
            if (resultsUpdated)
                args.CompletedResult.RecalculateStatistics();
        }

        private bool AddResultsToDictionary(IFlsResult[] resultsFiles)
        {
            bool resultsUpdated = false;
            var addToAnalyze = new List<IFlsResult>();
            var verdictUpdated = new List<IAptVerdictContext>();
            var newUnknownFiles = new List<IFlsResult>();
            lock (_flsHashResults)
            {
                foreach (var r in resultsFiles)
                {
                    if (!_flsHashResults.ContainsKey(r.Sha1))
                    {
                        _flsHashResults[r.Sha1] = new AptVerdictContext(r.Sha1, r.Cloud);
                        if (r.IsUnknown())
                            newUnknownFiles.Add(r);
                        if (r.IsNotAnalyzed())
                            addToAnalyze.Add(r);
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
                        else if (item.Cloud != r.Cloud &&
                                 (item.Cloud == CloudVerdict.FailedToAnalyze || item.Cloud == CloudVerdict.NoVerdict))
                        {
                            item.Cloud = r.Cloud;
                            var clone = item.Clone();
                            verdictUpdated.Add(clone);
                        }
                    }
                }
            }
            var verdictUpdatedAny = verdictUpdated.Any();
            Dictionary<string, CloudVerdict> dict;
            if (verdictUpdatedAny)
            {
                dict = verdictUpdated.ToDictionary((t) => t.Sha1, v => v.Cloud);
                UpdateLastResult(dict);
            }
            if ((verdictUpdatedAny || newUnknownFiles.Any()) && _isValkyrieStarted)
            {
                dict = verdictUpdated.Where(v => v.IsUnknown())
                    .Concat(newUnknownFiles)
                    .ToDictionary(t => t.Sha1, v => v.Cloud);
                List<ValkyrieFileInfo> filesToAnalyze = GetFilesToAnalyzeForValkyrie(dict);
                if (filesToAnalyze.Any())
                {
                    var valkyrieAnalysisResults = GetFullResult(filesToAnalyze.Select(v =>
                            new ValkyrieAnalysisResult(v.Sha1, ValkyrieVerdict.Pending)), GetResultsEnumerator().ToArray())
                        .Values;
                    _valkyrieClient.AddFilesToAnalyze(filesToAnalyze);
                    foreach (var fileInfo in valkyrieAnalysisResults)
                        ValkyrieAnalysisResult.TryAdd(fileInfo.Sha1, fileInfo);
                    ValkyrieOperationUid = Guid.NewGuid();
                }
            }
            if (verdictUpdatedAny)
                OnFlsAnalyzeProgressChangedAsync(new FlsRequestEventArgs<IAptVerdictContext>(verdictUpdated.ToArray(), null));
            if (addToAnalyze.Any())
               _flsManager.Enqueue(addToAnalyze.Select(f => f.Sha1), true);
            //  temp.Select(t)
            return resultsUpdated;
        }

        private List<ValkyrieFileInfo> GetFilesToAnalyzeForValkyrie(Dictionary<string, CloudVerdict> dict)
        {
            IEnumerable<IAptScanResult> results = GetResultsEnumerator().ToArray();
            IAptScanResult[] aptScanResults = results as IAptScanResult[] ?? results.ToArray();
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

        public FileResult[] GetModifiedValkyrieVerdicts(DateTime? modifiedDate)
        {
            FileResult[] fileResults;
            lock (ValkyrieAnalysisResult)
            {
                fileResults = ValkyrieAnalysisResult.Values.ToArray();
            }
            return (modifiedDate == null ? fileResults
                : fileResults.Where(vr => vr.VerdictModified > modifiedDate)).ToArray();
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
                _isValkyrieStarted = false;
                _valkyrieClient.AnalyzeAsyncCancel();
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
        }

        public void UpdateFlsPorts(int udpPort, int tcpPort)
        {
            _flsManager.UpdateFlsPorts(udpPort, tcpPort);
        }

        protected virtual void OnFlsAnalyzeProgressChangedAsync(FlsRequestEventArgs<IAptVerdictContext> e)
        {
            Action action = () =>
            {
                lock (_verdictUpdateHistory)
                    foreach (var result in e.Results)
                    {
                        _verdictUpdateHistory.Add(result);
                    }
                FlsAnalyzeProgressChanged?.Invoke(this, e);
            };
            Task.Factory.StartNew(action, TaskCreationOptions.None);
        }
    }

}
