using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using APTAT.Core.Apt;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Reports;
using APTAT.Core.Valkyrie;
using ValkyrieVerdict = APTAT.Core.Valkyrie.ValkyrieVerdict;

namespace APTAT.Core
{
    public interface IAptScanner
    {
        void BeginScan(object state = null);
        event EventHandler<ScanStartedEventArgs> ScanStarted;
        event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        event EventHandler<AptScanInProgressEventArgs> ScanInProgress;
    }

    public class ScanStartedEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public Guid OperationUid { get; set; }
        public object State { get; set; }
    }

    public class ScanCompletedEventArgs : EventArgs
    {
        public Guid OperationUid { get; set; }
        public int TotalCount { get; set; }
        public int NotPerformedCount { get; set; }
        public int SucceededCount { get; set; }
        public int FailedCount { get; set; }
        public int CalceledCount { get; set; }

        public FilesStatistics FilesStatistics { get; set; }
    }

    public class ScanInProgressEventArgs : EventArgs
    {
        public Guid OperationUid { get; set; }
        public double Percentage { get; set; }
    }
    public class AptScanInProgressEventArgs : ScanInProgressEventArgs
    {
        public DescentProgressItem[] ProgressItems { get; set; }
    }


    public class AptAtManager : IDisposable
    {
        private readonly IDescentSubSystem _descentSubSystem;
        public event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<FileResult>> ValkyrieAnalyzeProgressChanged;
        public event AnalyzeCompletedEventHandler<FileResult> ValkyrieAnalyzeCompleted;
        public event EventHandler ValkyrieAnalyzeStarted;
        private IValkyrieClient _valkyrieClient;
        private List<FileResult> _valkyrieAnalyzezResults;
        public List<FileResult> ValkyrieAnalysisResult
        {
            get { return _valkyrieAnalyzezResults; }
            set
            {
                _valkyrieAnalyzezResults = value;
                ValkyrieOperationUid = Guid.NewGuid();
            }
        }
        public Guid ValkyrieOperationUid { get; set; }
   

        public AptAtManager()
        {
            var catalog = SubSystemFactory.Current.Catalog;
            _descentSubSystem = catalog.Find<IDescentSubSystem>();
            _valkyrieClient = new ValkyrieClient();
        }

        void OnValkyrieAnalyzeStarted(object sender, EventArgs e)
        {
            var handler = ValkyrieAnalyzeStarted;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        void OnValkyrieManagerAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<ValkyrieAnalysisResult> e)
        {
            if (_valkyrieClient != null)
            {
                if (!e.IsPendingExist)
                    _valkyrieClient.AnalyzeProgressChanged -= OnValkyrieClientProgressChanged;
                _valkyrieClient.AnalyzeCompleted -= OnValkyrieManagerAnalyzeCompleted;
                _valkyrieClient.AnalyzeStarted -= OnValkyrieAnalyzeStarted;
            }
            var results = _descentSubSystem.GetFirstScanResults().ScanResults;
            var valkyrieAnalysisResult = GetFullResult(e.Result, results);
            var handler = ValkyrieAnalyzeCompleted;
            ValkyrieAnalysisResult = valkyrieAnalysisResult;
            if (handler != null)
            {
                var arg = new AnalyzeCompletedEventArgs<FileResult>(valkyrieAnalysisResult.ToArray(), e.Error, e.Cancelled
                    , e.IsPendingExist, e.UserState);
                handler(this, arg);
            }
        }

        void OnValkyrieClientProgressChanged(object sender, ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieAnalysisResult> e)
        {
            if (e.IsUpdate && !e.IsPendingExist)
            {
                _valkyrieClient.AnalyzeProgressChanged -= OnValkyrieClientProgressChanged;
            }
            var handler = ValkyrieAnalyzeProgressChanged;
            if (handler != null)
            {
                ProgressInfo<ValkyrieAnalysisResult> info = e.ProgressInfo;
                
                var results = _descentSubSystem.GetFirstScanResults().ScanResults;
                var valkyrieAnalysisResult = GetFullResult(info.Results, results);
                var malwareFilesCount =
                        valkyrieAnalysisResult.Where(i => i.ValkyrieAnalysisResult != null)
                            .Count(i => i.ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.Malware ||
                                i.ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.PotentiallyUnwantedApplication);

                var valkyrieAnalysisResultInternal = ValkyrieAnalysisResult;
                if (e.IsUpdate && valkyrieAnalysisResultInternal != null)
                {
                    foreach (var fileResult in valkyrieAnalysisResult)
                    {
                        valkyrieAnalysisResultInternal.Remove(fileResult);
                        valkyrieAnalysisResultInternal.Add(fileResult);
                    }
                    if (valkyrieAnalysisResult.Any())
                        ValkyrieOperationUid = Guid.NewGuid();
                }

                var unknownFilesCount = valkyrieAnalysisResult.Count - malwareFilesCount;

                ProgressInfo<FileResult> progressInfo = new ProgressInfo<FileResult>(info.TotalFileCount, info.ProcessedFileCount, malwareFilesCount, unknownFilesCount, valkyrieAnalysisResult.ToArray());
                handler(this, new ValkyrieAnalyzeProgressChangedEventArgs<FileResult>(e.Percent, progressInfo, e.IsUpdate, e.IsPendingExist));
            }
        }

        private static List<FileResult> GetFullResult(IEnumerable<ValkyrieAnalysisResult> valkyrieResult, ConcurrentDictionary<Guid, AptScanResult> scanResults)
        {
            List<FileResult> fileResults = new List<FileResult>();

            var valkyrieAnalysisResults = valkyrieResult as ValkyrieAnalysisResult[] ?? valkyrieResult.ToArray();
            foreach (var scanResult in scanResults)
            {
                foreach (var badFile in scanResult.Value.BadFiles)
                {
                    var found = fileResults.SingleOrDefault(f => f.Sha1 == badFile.Sha1);

                    if (found == null)
                    {
                        found = new FileResult(badFile);
                        var vResult = valkyrieAnalysisResults.FirstOrDefault(v => v.SHA1 != null && v.SHA1.ToUpper() == badFile.Sha1.ToUpper());

                        if (vResult != null)
                        {
                            found.ValkyrieAnalysisResult = vResult;
                            fileResults.Add(found);
                        }
                    }
                  
                    if (scanResult.Value.AptReport != null && scanResult.Value.AptReport.Computer != null)
                    {
                        var computer = scanResult.Value.AptReport.Computer;
                        var comp = new ComputerInformation(computer, scanResult.Value.TargetUid);
                        if (!found.Computers.Any(c => c.IP == comp.IP))
                        {                        
                            comp.FilePath = badFile.Path;
                            found.Computers.Add(comp);
                        }
                    }
                }
            }
            return fileResults;
        }

        public static List<ValkyrieFileInfo> GetDistinctFileInfo(ConcurrentDictionary<Guid, AptScanResult> results)
        {
            Dictionary<string, AptScanResult> scanResults = results.Select(r => r.Value).ToDictionary(t => t.TargetUid.ToString());

            List<ValkyrieFileInfo> files = new List<ValkyrieFileInfo>();
            foreach (var scanResult in scanResults)
            {
                foreach (var badFile in scanResult.Value.BadFiles)
                {
                    if (files.All(f => f.Sha1 != badFile.Sha1))
                    {
                        files.Add(new ValkyrieFileInfo() { Ip = scanResult.Value.IpAddress, Path = badFile.Path, Sha1 = badFile.Sha1, Size = badFile.Size });
                    }
                }
            }
            return files;
        }
        public static List<ValkyrieFileInfo> GetDistinctUknownFileInfo(ConcurrentDictionary<Guid, AptScanResult> results)
        {
            Dictionary<string, AptScanResult> scanResults = results.Select(r => r.Value).ToDictionary(t => t.TargetUid.ToString());

            List<ValkyrieFileInfo> files = new List<ValkyrieFileInfo>();
            foreach (var scanResult in scanResults)
            {
                foreach (var badFile in scanResult.Value.GetUnknownFiles())
                {
                    if (files.All(f => f.Sha1 != badFile.Sha1))
                    {
                        files.Add(new ValkyrieFileInfo()
                        {
                            Ip = scanResult.Value.IpAddress,
                            Path = badFile.Path,
                            Sha1 = badFile.Sha1,
                            Size = badFile.Size,
                            FlsCloud = badFile.Cloud
                        });
                    }
                }
            }
            return files;
        }
        public IAptScanner GetAptScanner(DeploymentTargetsCollection targets, string login, string password, 
            DevicesSourceDescription devicesSourceDescription, string scanScope, string tcp, string udp)
        {
            if (scanScope == null)
                scanScope = Properties.Settings.Default.ScanScope;

            
            var scanner = new AptScanner(_descentSubSystem, targets, login, password, devicesSourceDescription, scanScope, tcp, udp);
            scanner.ScanStarted += OnScanStarted;
            scanner.ScanCompleted += OnScanCompleted;
            return scanner;
        }

        private void OnScanStarted(object sender, ScanStartedEventArgs e)
        {
            ValkyrieAnalysisResult = null;
            _valkyrieClient.AnalyzeAsyncCancel();
        }

        private void OnScanCompleted(object sender, ScanCompletedEventArgs e)
        {
            AptScanner scanner = sender as AptScanner;
            if (scanner != null)
            {
                scanner.ScanStarted -= OnScanStarted;
                scanner.ScanCompleted -= OnScanCompleted;
            }
        }

        public IAptReportBuilder GetAptReportBuilder()
        {
            IAptReportBuilder aptReportBuilder = new AptReportBuilder(_descentSubSystem, this);
            return aptReportBuilder;
        }

        public AptScanResult GetScanResultByTargetUid(Guid targetUid)
        {
            AptScanResult aptScanResult = null;
            var results = _descentSubSystem.GetFirstScanResults();
            if (results != null)
            {
                results.ScanResults.TryGetValue(targetUid, out aptScanResult);
            }

            return aptScanResult;
        }

        public void StopScan()
        {
            _descentSubSystem.AbortAllThreads();
            _valkyrieClient.AnalyzeAsyncCancel();
        }
        

        public void StartValkyrieAnalysis(string login, string password, 
            string camLicenseId, string camUserName, string camPassword, NetworkCredential proxyCredential = null)
        {
            var camRegistration = new CamRegistration(camLicenseId, camUserName, camPassword);
            var results = _descentSubSystem.GetFirstScanResults().ScanResults;
            var distinctFileList = GetDistinctUknownFileInfo(results);
            _valkyrieClient.AnalyzeProgressChanged -= OnValkyrieClientProgressChanged;
            _valkyrieClient.AnalyzeProgressChanged += OnValkyrieClientProgressChanged;
            _valkyrieClient.AnalyzeCompleted += OnValkyrieManagerAnalyzeCompleted;
            _valkyrieClient.AnalyzeStarted += OnValkyrieAnalyzeStarted;
            _valkyrieClient.StartAnalyze(camRegistration, distinctFileList, login, password,results.Count, proxyCredential);           
        }

        public string GetValkyrieDetailsLink()
        {
            return _valkyrieClient.GetValkyrieDetailsLink();
        }

        public int GetDistinctFileCount()
        {
            var results = _descentSubSystem.GetFirstScanResults().ScanResults;
            var files = GetDistinctFileInfo(results);
            return files.Count;
        }

        public void Dispose()
        {
            if (_valkyrieClient != null)
            {
                _valkyrieClient.Dispose();
                _valkyrieClient = null;
            }
        }
    }

}
