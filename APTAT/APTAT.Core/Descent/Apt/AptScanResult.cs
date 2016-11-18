using System;
using System.Linq;
using APTAT.Core.Reports;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;

namespace APTAT.Core.Descent.Apt
{
    public class AptScanResult : ComputerDescentResult, IAptScanResult
    {
        public AptScanResult(IComputer computer) : base(computer)
        {
            ResultType = ScanResultType.InProgress;
        }
        public string OsName
        {
            get
            {
                if (_aptReport != null && _aptReport.Computer != null && _aptReport.Computer.Os != null)
                    return _aptReport.Computer.Os.ToString();
                return null;
            }
        }
        public string FileName { get; set; }
        public int TotalFilesCount { get; set; }
        public int UnknownFilesCount { get; set; }
        public int SafeFilesCount { get; set; }
        public AptScanStage ScanStage { get; set; }
        private int _malwareCount;
        public int MalwareFilesCount {
            get            
            {
                return _malwareCount;
            }
            set
            {
                _malwareCount = value;
            }
        }
        public int EmptyFilesCount { get; set; }

        public int LookupInProgressFilesCount => FailedToAnalyzeFilesCount + NoVerdictFilesCount;
        
        public void RecalculateStatistics()
        {
            Int32 unknownFiles = 0;
            int safeFileCount = 0;
            Int32 malwareFiles = 0;
            Int32 failedToAnalyzeFiles = 0;
            Int32 noVerdictFiles = 0;

            foreach (var file in Files)
            {
                switch (file.Cloud)
                {
                    case CloudVerdict.Safe:
                        safeFileCount++;
                        break;
                    case CloudVerdict.Unknown:
                    case CloudVerdict.FirstSeen:
                        unknownFiles++;
                        break;
                    case CloudVerdict.Malware:
                        malwareFiles++;
                        break;
                    case CloudVerdict.FailedToAnalyze:
                        failedToAnalyzeFiles++;
                        break;
                    case CloudVerdict.NoVerdict:
                        noVerdictFiles++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            UnknownFilesCount = unknownFiles;
            NoVerdictFilesCount = noVerdictFiles;
            FailedToAnalyzeFilesCount = failedToAnalyzeFiles;
            MalwareFilesCount = malwareFiles;
            SafeFilesCount = safeFileCount;

        }

        public int FailedToAnalyzeFilesCount { get; set; }       
        public int NoVerdictFilesCount { get; set; }
        public int PendingAnalysisFilesCount { get; set; }
        public int TotalErrorFilesCount
        {
            get
            {
                return this.FailedToAnalyzeFilesCount + this.NoVerdictFilesCount;
            }
        }
        
        public int SkippedFilesCount { get; set; }
        public int RunningProgramsCount { get; set; }
        public int AutorunsCount { get; set; }
        public string ResultErrorText { get; set; }
        public ScanResultType ResultType { get; set; }


        public int BadFilesCount
        {
            get { return UnknownFilesCount + MalwareFilesCount; }
        }

        public TargetVerdict TargetVerdict
        {
            get
            {
                if (ResultType == ScanResultType.Failed || ResultType == ScanResultType.Canceled)
                    return TargetVerdict.Unknown;
                if (MalwareFilesCount > 0)
                    return TargetVerdict.Infected;
                if (UnknownFilesCount > 0 || FailedToAnalyzeFilesCount > 0 || NoVerdictFilesCount > 0 || PendingAnalysisFilesCount > 0)
                    return TargetVerdict.Vulnerable;
                if (ResultType == ScanResultType.NotPerformed)
                    return TargetVerdict.NotScanned;
                return TargetVerdict.Safe;
            }
        }

        public string TargetVerdictString
        {
            get
            {
                switch (TargetVerdict)
                {
                    case TargetVerdict.Unknown:
                        return "Unknown";
                    case TargetVerdict.Safe:
                        return "Safe";
                    case TargetVerdict.Infected:
                        return "Infected";
                    case TargetVerdict.Vulnerable:
                        return "At Risk";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public Module[] Files
        {
            get
            {
                if (_aptReport != null)
                {

                    // Sort out empty files (size = 0 and hash = 000000000...)
                    if (_aptReport.Modules != null)
                    {
                        var notEmptyFiles = from file in _aptReport.Modules
                                            where !file.Sha1.StartsWith("0000000000000000000000000000000000000000")
                                            select file;
                        return notEmptyFiles.ToArray();
                    }
                }
                return new Module[0];
            }
        }
        

        public Module[] BadFiles
        {
            get
            {
                return GetBadFiles();
            }
        }


        public Module[] GetFilesByStatus(CloudVerdict cloudVerdict)
        {
            return (from file in Files where file.Cloud == cloudVerdict select file).ToArray();
        }

        public Module[] GetUnknownFiles()
        {
            return (from file in Files where file.Cloud == CloudVerdict.Unknown || file.Cloud == CloudVerdict.FirstSeen select file).ToArray();
        }


        public Module[] GetLookupInProgressFiles()
        {
            return (from file in Files where file.Cloud == CloudVerdict.FailedToAnalyze || file.Cloud == CloudVerdict.NoVerdict select file).ToArray();
        }
        public Module[] GetFailedToAnalyzeFiles()
        {            
            return (from file in Files where  file.Cloud == CloudVerdict.FailedToAnalyze select file).ToArray();
        }

        public Module[] GetNoVerdictFiles()
        {            
            return (from file in Files where file.Cloud == CloudVerdict.NoVerdict select file).ToArray();
        }

        public Module[] GetBadFiles()
        {
            var files = Files;
            if (files == null)
                return new Module[0];
            return (from file in files where file.IsBad select file).ToArray();
        }

        public Module[] GetUknownFiles()
        {
            var files = Files;
            if (files == null)
                return new Module[0];
            return (from file in files where file.IsUnknownOrFailed select file).ToArray();
        }

        public void SetAptReportInstance(AptReport report)
        {
            _aptReport = report;

            if (_aptReport != null)
            {
                RunningProgramsCount = _aptReport.Processes != null ? _aptReport.Processes.Length : 0;
                AutorunsCount = _aptReport.Autoruns != null ? _aptReport.Autoruns.Length : 0;
            }
        }

        private AptReport _aptReport;
        
        public AptReport AptReport
        {
            get { return _aptReport; }
        }

        internal Module[] GetPendingAnalysisFiles()
        {
            return new Module[] { };
        }

        public override void UpdateStatus(IDescentProgressItem progress)
        {
            base.UpdateStatus(progress);
            var item = progress as IAptDescentProgressItem;
            if (item != null)
            {
                ScanStage = item.ScanStage;
            }
            if (progress.IsCanceled)
                ScanStage = AptScanStage.OperationCanceled;
        }

        public override void UpdateStatus(IOperationDescentResult result)
        {
            base.UpdateStatus(result);
            if (result.IsCanceled)
                ScanStage = AptScanStage.OperationCanceled;
            if (Completed && !Succeeded && !IsCanceled)
                ScanStage = AptScanStage.OperationFailed;
            if (Completed && Succeeded && !IsCanceled)
                ScanStage = AptScanStage.OperationCompleted;
            if (Completed && !Succeeded && !IsCanceled)
                ScanStage = AptScanStage.OperationFailed;
        }
    }

    
}
