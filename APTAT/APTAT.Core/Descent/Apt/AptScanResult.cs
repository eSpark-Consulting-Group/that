using System;
using System.Linq;
using System.Runtime.CompilerServices;
using APTAT.Core.Reports;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;

namespace APTAT.Core.Descent.Apt
{
    public class AptScanResult : ComputerDescentResult, IAptScanResultInternal
    {
        public AptScanResult(IComputer computer, Guid operationUid, ITimeStampProvider timeStampProvider) 
            : base(computer, operationUid, timeStampProvider)
        {
            ResultType = ScanResultType.InProgress;
        }
     
        public string FileName { get; set; }
        public int TotalFilesCount { get; set; }
        public int UnknownFilesCount { get; set; }
        public int SafeFilesCount { get; set; }
        public AptScanStage ScanStage { get; set; }

        public int MalwareFilesCount { get; set; }

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

        private IModule[] ScanModules
        {
            get
            {
                if (Files == null) return new ScanModule[] {};
                var q = from module in Files
                        where !module.Sha1.StartsWith("0000000000000000000000000000000000000000")
                        select new ScanModule()
                        {
                            Sha1 = module.Sha1,
                            Accessed = module.Accessed,
                            AccessedInternal = module.AccessedInternal,
                            Attributes = module.Attributes,
                            Cloud = module.Cloud,
                            CloudDate = module.CloudDate,
                            Copyright = module.Copyright,
                            Created = module.Created,
                            CreatedInternal = module.CreatedInternal,
                            Description = module.Description,
                            FileAge = module.FileAge,
                            FileAttributes = module.FileAttributes,
                            FileSizeFormatted = module.FileSizeFormatted,
                            Id = module.Id,
                            IsBad = module.IsBad,
                            IsUnknownOrFailed = module.IsUnknownOrFailed,
                            Modified = module.Modified,
                            ModifiedInternal = module.ModifiedInternal,
                            Path = module.Path,
                            Product = module.Product,
                            Signer = module.Signer,
                            Size = module.Size,
                            Version = module.Version
                        };
                return q.ToArray();
            }
        }

        public IModule[] Files
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
                return new IModule[0];
            }
        }
        

        public IModule[] BadFiles
        {
            get
            {
                return GetBadFiles();
            }
        }


        public IModule[] GetFilesByStatus(CloudVerdict cloudVerdict)
        {
            return (from file in Files where file.Cloud == cloudVerdict select file).ToArray();
        }

        public IModule[] GetBadFiles()
        {
            var files = Files;
            if (files == null)
                return new IModule[0];
            return (from file in files where file.IsBad select file).ToArray();
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

        internal IModule[] GetPendingAnalysisFiles()
        {
            return new IModule[] { };
        }

        public override void UpdateStatus(IDescentProgressItem progress)
        {
            base.UpdateStatus(progress);
            var item = progress as IAptDescentProgressItem;
            if (item != null)
            {
                ScanStage = item.ScanStage;
                UnknownFilesCount = item.UnknownFilesCount;
                TotalFilesCount = item.TotalFilesCount;
                SafeFilesCount = item.SafeFilesCount;
                MalwareFilesCount = item.MalwaresCount;
                FailedToAnalyzeFilesCount = item.ErrorFilesCount;
                NoVerdictFilesCount = item.FailedAtNodeCount;
            }
            if (progress.IsCanceled)
            {
                ScanStage = AptScanStage.OperationCanceled;
                Modified = TimeStampProvider.GetCurretTimestamp();
            }
        }

        public override void UpdateStatus(IOperationDescentResult result)
        {
            base.UpdateStatus(result);
            if (result.IsCanceled)
                ScanStage = AptScanStage.OperationCanceled;
            if (Completed && !Succeeded && !IsCanceled)
                ScanStage = AptScanStage.OperationFailed;
            if (Completed && Succeeded && !IsCanceled)
                ScanStage = AptScanStage.RemoteOperationCompleted;
            Modified = TimeStampProvider.GetCurretTimestamp();
        }

        public void SetCompleted()
        {
            if (ScanStage == AptScanStage.RemoteOperationCompleted)
                ScanStage = AptScanStage.OperationCompleted;
            Modified = TimeStampProvider.GetCurretTimestamp();
        }

        public IModule[] MalwareFiles
        {
            get
            {
                return (from file in ScanModules where file.Cloud == CloudVerdict.Malware select file).ToArray();
            }
        }
        public IModule[] UnknownFiles
        {
            get
            {
                return (from file in ScanModules where file.Cloud == CloudVerdict.Unknown || file.Cloud == CloudVerdict.FirstSeen select file).ToArray();
            }
        }

        public IModule[] LookupInProgressFiles
        {
            get
            {
                return (from file in ScanModules where file.Cloud == CloudVerdict.FailedToAnalyze || file.Cloud == CloudVerdict.NoVerdict select file).ToArray();
            }
        }

        public IModule[] PendingFiles
        {
            get
            {
                return (from file in Files where file.Cloud == CloudVerdict.FailedToAnalyze || file.Cloud == CloudVerdict.NoVerdict select file).ToArray();
            }
        }
  
    }


}
