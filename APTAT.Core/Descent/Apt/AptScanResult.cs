using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.Core.Reports;

namespace APTAT.Core.Descent.Apt
{
    public class AptScanResult 
    {
        public Guid TargetUid { get; set; }
        public string HostName { get; set; }
        public string IpAddress { get; set; }
        public string OsName
        {
            get
            {
                if (_aptReport != null && _aptReport.Computer != null && _aptReport.Computer.Os != null)
                    return _aptReport.Computer.Os.ToString();
                return null;
            }
        }
        public string OrganizationUnit { get; set; }
        public string FileName { get; set; }
        public int TotalFilesCount { get; set; }
        public int UnknownFilesCount { get; set; }
        public int SafeFilesCount { get; set; }
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

        public double ProgressPercenage { get; set; }

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
                return null;
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
    }

    
}
