using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using APTAT.Common;
using APTAT.Core.Apt;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Reports;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Descent.Exceptions;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using ValkyrieVerdict = APTAT.Valkyrie.ValkyrieVerdict;
using APTAT.Valkyrie;

namespace APTAT.Core
{


    public class AptAtManager : IDisposable
    {
        private readonly IDescentSubSystem _descentSubSystem;
        private IAptScanner _scanner;
        public event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<FileResult>> ValkyrieAnalyzeProgressChanged;
        public event AnalyzeCompletedEventHandler<FileResult> ValkyrieAnalyzeCompleted;
        public event EventHandler ValkyrieAnalyzeStarted;
        private IValkyrieClient _valkyrieClient;
        private List<FileResult> _valkyrieAnalyzezResults;
        private ConcurrentDictionary<Guid, AptScanResult> _lastAptResults;

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
            _descentSubSystem = CompositionHelper.Instance.Resolve<IDescentSubSystem>();
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
            var valkyrieAnalysisResult = GetFullResult(e.Result, _lastAptResults);
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
                
                var valkyrieAnalysisResult = GetFullResult(info.Results, _lastAptResults);
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
                var modules = scanResult.Value.GetUnknownFiles();
                
                foreach (var badFile in modules)
                {
                    if (files.All(f => f.Sha1 != badFile.Sha1))
                    {
                        files.Add(new ValkyrieFileInfo()
                        {
                            Ip = scanResult.Value.IpAddress,
                            Path = badFile.Path,
                            Sha1 = badFile.Sha1,
                            Size = badFile.Size,
                            FlsCloud = MapCloud(badFile.Cloud)
                        });
                    }
                }
            }
            return files;
        }

        private static Valkyrie.CloudVerdict MapCloud(Descent.Apt.CloudVerdict cloud)
        {
            switch (cloud)
            {
                case Descent.Apt.CloudVerdict.Safe:
                    return Valkyrie.CloudVerdict.Safe;                    
                case Descent.Apt.CloudVerdict.Unknown:
                    return Valkyrie.CloudVerdict.Unknown;
                    
                case Descent.Apt.CloudVerdict.Malware:
                    return Valkyrie.CloudVerdict.Malware;
                    
                case Descent.Apt.CloudVerdict.FirstSeen:
                    return Valkyrie.CloudVerdict.FirstSeen;
                    
                case Descent.Apt.CloudVerdict.FailedToAnalyze:
                    return Valkyrie.CloudVerdict.FailedToAnalyze;
                    
                case Descent.Apt.CloudVerdict.NoVerdict:
                    return Valkyrie.CloudVerdict.NoVerdict;
                    
                default:
                    throw new NotSupportedException("Can not convert to Valkyrie.CloudVerdict from Apt.CloudVerdict " + cloud);
            }
        }

        public IAptScanner GetAptScanner(DeploymentTargetsCollection targets, string login, string password, 
            IDevicesSourceDescription devicesSourceDescription, ScanOptions options)
        {           

            if (_scanner != null) throw new AnotherScanIsInProgressException();
            _scanner = new AptScanner(_descentSubSystem, targets, login, password, devicesSourceDescription, options);
            _scanner.ScanStarted += OnScanStarted;
            _scanner.ScanCompleted += OnScanCompleted;
            return _scanner;
        }

        private void OnScanStarted(object sender, ScanStartedEventArgs e)
        {
            ValkyrieAnalysisResult = null;
            _valkyrieClient.AnalyzeAsyncCancel();
            _lastAptResults = _scanner.ScanOperationContext.ScanResults;
        }

        private void OnScanCompleted(object sender, ScanCompletedEventArgs e)
        {
            AptScanner scanner = sender as AptScanner;
            if (scanner != null)
            {
                scanner.ScanStarted -= OnScanStarted;
                scanner.ScanCompleted -= OnScanCompleted;
                var aptResult = (AptScanOperationContext)_descentSubSystem.GetDescentResultsByOperationUid(e.OperationUid);
                _lastAptResults = aptResult.ScanResults;
                _scanner = null;
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
            if (_lastAptResults != null)
            {
                _lastAptResults.TryGetValue(targetUid, out aptScanResult);
            }

            return aptScanResult;
        }

        public void StopScan()
        {
            _descentSubSystem.StopAllScans(0);
            _valkyrieClient.AnalyzeAsyncCancel();
        }
        

        public void StartValkyrieAnalysis(string login, string password, 
            string camLicenseId, string camUserName, string camPassword, NetworkCredential proxyCredential = null)
        {
            var camRegistration = new CamRegistration(camLicenseId, camUserName, camPassword);
            var distinctFileList = GetDistinctUknownFileInfo(_lastAptResults);
            _valkyrieClient.AnalyzeProgressChanged -= OnValkyrieClientProgressChanged;
            _valkyrieClient.AnalyzeProgressChanged += OnValkyrieClientProgressChanged;
            _valkyrieClient.AnalyzeCompleted += OnValkyrieManagerAnalyzeCompleted;
            _valkyrieClient.AnalyzeStarted += OnValkyrieAnalyzeStarted;
            _valkyrieClient.StartAnalyze(camRegistration, distinctFileList, login, password, _lastAptResults.Count, proxyCredential);           
        }

        public string GetValkyrieDetailsLink()
        {
            return _valkyrieClient.GetValkyrieDetailsLink();
        }

        public int GetDistinctFileCount()
        {
            var files = GetDistinctFileInfo(_lastAptResults);
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
