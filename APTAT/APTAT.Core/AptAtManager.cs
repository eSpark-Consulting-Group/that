using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using APTAT.Core.Apt;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Reports;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using ValkyrieVerdict = APTAT.Valkyrie.ValkyrieVerdict;
using APTAT.Valkyrie;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;
using System.Collections.ObjectModel;
using APTAT.Core.Properties;

namespace APTAT.Core
{
    public class AptAtManager : IAptAtManagerInternal
    {
        private readonly IDescentSubSystem _descentSubSystem;
        private readonly IAptScannerFactory _aptScannerFactory;
        private IAptScanner _scanner;
        private readonly IFlsManager _flsManager;
        public event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<FileResult>> ValkyrieAnalyzeProgressChanged;
        public event EventHandler<FlsRequestEventArgs> FlsAnalyzeProgressChanged;
        public event AnalyzeCompletedEventHandler<FileResult> ValkyrieAnalyzeCompleted;
        public event EventHandler ValkyrieAnalyzeStarted;
        private IValkyrieClient _valkyrieClient;
        private Collection<IAptScanOperationContext> _lastAptResults = new Collection<IAptScanOperationContext>();

        public Collection<IAptScanOperationContext> AptScanOperationContexts
        {
            get { return _lastAptResults; }
        }

        public ConcurrentDictionary<string, FileResult> ValkyrieAnalysisResult { get; }

        private IEnumerable<IAptScanResult> GetResultsEnumerator()
        {
            foreach (var result in _lastAptResults)
                foreach (var scan in result.ScanResults)
                    yield return scan;
        }


        private void UpdateLastResult(Dictionary<string, CloudVerdict> resultsWithoutErrors)
        {
            
            bool isUpdated = false;
            foreach (var result in GetResultsEnumerator().ToArray())
            {
                var files = result.Files.Where(f => f.Cloud == CloudVerdict.FailedToAnalyze || f.Cloud == CloudVerdict.NoVerdict).ToArray();

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

        public Guid ValkyrieOperationUid { get; set; }
        public Guid FlsLookupOperationUid { get; set; }

        public AptAtManager(IDescentSubSystem descentSubSystem, IValkyrieClient valkyrieClient, IFlsManager manager,
            IAptScannerFactory aptScannerFactory, IDistinctFileHelper distinctHelper)
        {
            _descentSubSystem = descentSubSystem;
            _aptScannerFactory = aptScannerFactory;
            _distinctHelper = distinctHelper;

            ValkyrieAnalysisResult = new ConcurrentDictionary<string, FileResult>(StringComparer.OrdinalIgnoreCase);
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
            {
                ValkyrieAnalysisResult.AddOrUpdate(fileResult.Sha1, fileResult, (s, result) => fileResult);
            }
            ValkyrieOperationUid = Guid.NewGuid();
            var arg = new AnalyzeCompletedEventArgs<FileResult>(valkyrieAnalysisResult.ToArray(), e.Error, e.Cancelled
                , e.UserState);

            lock(_lastAptResults)
                _lastAptResults.Clear();

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
                if (e.IsUpdate && valkyrieAnalysisResultInternal != null)
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

        private int GetDistinctFilesCount(IEnumerable<IAptScanResult> results)
        {
            var files = new List<ValkyrieFileInfo>();
            foreach (var scanResult in results)
            {
                if (scanResult.IsCanceled)
                    continue;

                foreach (var badFile in scanResult.BadFiles)
                {
                    if (files.All(f => !String.Equals(f.Sha1, badFile.Sha1, StringComparison.OrdinalIgnoreCase)))
                    {
                        files.Add(new ValkyrieFileInfo()
                        {
                            Sha1 = badFile.Sha1
                        });
                    }
                }
            }
            return files.Count;
        }

        private List<ValkyrieFileInfo> GetDistinctUknownFileInfo(IAptScanResult[] results)
        {
            return _distinctHelper.GetDistinctUknownFileInfo(results);
        }      

        public IAptScanner GetAptScanner(IDevicesSourceDescription devicesSourceDescription, ScanOptions options)
        {
            if (_scanner != null)
                return _scanner;

            if (options.ScanInit)
            {
                _lastAptResults.Clear();
                ValkyrieAnalysisResult.Clear();
            }

            _flsManager.UpdateFlsPorts(int.Parse(options.UdpPort), int.Parse(options.TcpPort));

            _scanner = _aptScannerFactory.CreateAptScanner(_descentSubSystem, devicesSourceDescription, options);
            _scanner.ScanStarted += OnScanStarted;
            _scanner.ComputerScanCompleted += OnScannerOnScanCompleted;
            _scanner.ScanCompleted += OnScanCompleted;

            return _scanner;
        }
        
        private bool _isValkyrieStarted;

        private void OnScannerOnScanCompleted(object sender, ComputerScanCompletedEventArgs args)
        {
            if (args.CompletedResult.IsCanceled)
                return;

            var resultsUpdated = AddResultsToDictionary(args.CompletedResult.Files.ToArray());
            if (resultsUpdated)
                args.CompletedResult.RecalculateStatistics();
        }

        private bool AddResultsToDictionary(IFlsResult[] resultsFiles)
        {
            bool resultsUpdated = false;
            var addToAnalyze = new List<IFlsResult>();
            var verdictUpdated = new List<IFlsResult>();

            foreach (var r in resultsFiles)
            {
                lock (_flsHashResults)
                {
                    if (!_flsHashResults.ContainsKey(r.Sha1))
                    {
                        _flsHashResults[r.Sha1] = r.Cloud;
                        if (r.IsNotAnalyzed())
                            addToAnalyze.Add(r);
                    }
                    else
                    {
                        var item = _flsHashResults[r.Sha1];
                        if (r.IsNotAnalyzed())
                        {
                            if (item != CloudVerdict.FailedToAnalyze &&
                                item != CloudVerdict.NoVerdict)
                            {
                                r.Cloud = item;
                                resultsUpdated = true;
                            }
                        }
                        else if (item != r.Cloud &&
                            (item == CloudVerdict.FailedToAnalyze || item == CloudVerdict.NoVerdict))
                        {
                            _flsHashResults[r.Sha1] = r.Cloud;
                            verdictUpdated.Add(r);
                        }
                    }
                }
            }

            if (verdictUpdated.Any())
            {
                Dictionary<string, CloudVerdict> dict = verdictUpdated.ToDictionary((t) => t.Sha1, v => v.Cloud);
                UpdateLastResult(dict);
                if (_isValkyrieStarted)
                {
                    dict = verdictUpdated.Where(v => v.Cloud == CloudVerdict.FirstSeen || v.Cloud == CloudVerdict.Unknown)
                        .ToDictionary(t => t.Sha1, v => v.Cloud);
                    List<ValkyrieFileInfo> filesToAnalyze = GetFilesToAnalyzeForValkyrie(dict);
                    if (filesToAnalyze.Any())
                    {
                        var valkyrieAnalysisResults = GetFullResult(filesToAnalyze.Select(v =>
                        new ValkyrieAnalysisResult(v.Sha1, ValkyrieVerdict.Pending)), GetResultsEnumerator().ToArray()).Values;
                        _valkyrieClient.AddFilesToAnalyze(filesToAnalyze);
                        foreach (var fileInfo in valkyrieAnalysisResults)
                            ValkyrieAnalysisResult.TryAdd(fileInfo.Sha1, fileInfo);
                        ValkyrieOperationUid = Guid.NewGuid();
                    }
                }
                OnFlsAnalyzeProgressChanged(new FlsRequestEventArgs(verdictUpdated.ToArray(), null));
            }
            
            if (addToAnalyze.Any())
            {
                _flsManager.Enqueue(addToAnalyze.Select(f => f.Sha1), true);
            }

            return resultsUpdated;
        }

        private List<ValkyrieFileInfo> GetFilesToAnalyzeForValkyrie(Dictionary<string, CloudVerdict> dict)
        {
            IAptScanResult[] aptScanResults = GetResultsEnumerator().ToArray();
            List<ValkyrieFileInfo> distinctFileList = GetDistinctUknownFileInfo(aptScanResults);
            List<ValkyrieFileInfo> fileInfoList = distinctFileList.Where(x => dict.ContainsKey(x.Sha1)).ToList();

            return fileInfoList;
        }

        private readonly  Dictionary<string, CloudVerdict> _flsHashResults = new Dictionary<string, CloudVerdict>();
        private readonly IDistinctFileHelper _distinctHelper;

        private void FlsManagerOnAnalyzed(object sender, FlsRequestEventArgs e)
        {
            if (e.Results.Any())
            {
                AddResultsToDictionary(e.Results);
            }
        }

        private void OnScanStarted(object sender, ScanStartedEventArgs e)
        {
            if (e.RescanAttempt || _lastAptResults.Any())
                return;

            _flsManager.Cancel();
            _isValkyrieStarted = false;
            _valkyrieClient.AnalyzeAsyncCancel();
            lock (_flsHashResults)
            {
                _flsHashResults.Clear();
            }
        }

        private void OnScanCompleted(object sender, ScanCompletedEventArgs e)
        {
            IAptScanner scanner = sender as IAptScanner;
            if (scanner != null)
            {
                scanner.ScanStarted -= OnScanStarted;
                scanner.ScanCompleted -= OnScanCompleted;
                _scanner = null;

                var aptResult =
                    (IAptScanOperationContext) _descentSubSystem.GetDescentResultsByOperationUid(e.OperationUid);

                lock(_lastAptResults)
                    _lastAptResults.Add(aptResult);
            }
        }

        public IAptReportBuilder GetAptReportBuilder()
        {
            IAptReportBuilder aptReportBuilder = new AptReportBuilder(this);
            return aptReportBuilder;
        }

        public void StopScan(Guid[] targets)
        {
            if (targets.Any())
                _descentSubSystem.StopOperationForTargetUid(targets);
            else
            {
                _descentSubSystem.StopAllScans(0);

                _flsManager.Cancel();
                _isValkyrieStarted = false;
                _valkyrieClient.AnalyzeAsyncCancel();
                lock (_flsHashResults)
                {
                    _flsHashResults.Clear();
                }
            }
        }


        public void StartValkyrieAnalysis(string login, string password,
            string camLicenseId, string camUserName, string camPassword, NetworkCredential proxyCredential)
        {
            lock (_flsHashResults)
            {
                var camRegistration = new CamRegistration(camLicenseId, camUserName, camPassword);
                var aptScanResults = GetResultsEnumerator().ToArray();
                var distinctFileList = GetDistinctUknownFileInfo(aptScanResults);
                _valkyrieClient.StartAnalyze(camRegistration, distinctFileList, login, password, aptScanResults.Length,
                    proxyCredential);
            }
        }

        public string GetValkyrieDetailsLink()
        {
            return _valkyrieClient.GetValkyrieDetailsLink();
        }

        public int GetDistinctFilesCount()
        {
            return GetDistinctFilesCount(GetResultsEnumerator().ToArray());
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

        protected virtual void OnFlsAnalyzeProgressChanged(FlsRequestEventArgs e)
        {
            FlsAnalyzeProgressChanged?.Invoke(this, e);
        }
    }

}
