using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;
using OperatingSystem = APTAT.Core.Descent.Apt.OperatingSystem;

namespace APTAT.Core
{
    public class ComputerInformation : IEquatable<ComputerInformation>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(String name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public ComputerInformation()
        {
            Files = new ObservableCollection<FileResult>();
        }

        public ComputerInformation(Computer computer, Guid targetUid)
        {
            Files = new ObservableCollection<FileResult>();
            Os = computer.Os;
            Name = computer.Name;
            Sid = computer.Sid;
            TargetUid = targetUid;
            if (computer.Connections.Any())
            {
                IP = computer.Connections.First().IPv4;
            }
        }

        public ComputerInformation(ComputerInformation computer)
        {
            Files = new ObservableCollection<FileResult>();
            Os = computer.Os;
            Name = computer.Name;
            Sid = computer.Sid;
            IP = computer.IP;
            FilePath = computer.FilePath;
            ScanCompleted = true;
            TargetUid = computer.TargetUid;
        }

        public string Name { get; set; }
        public string Sid { get; set; }
        public OperatingSystem Os { get; set; }
        public string IP { get; set; }
        public string FilePath { get; set; }
        public Guid TargetUid { get; set; }

        public string FullName
        {
            get
            {
                return String.Format("{0} ({1})", Name, IP);
            }
        }

        public string NameAndIpAddressSorting
        {
            get
            {
                if (string.IsNullOrEmpty(IP))
                    return (!char.IsDigit(Name[0]) ? "A" : "B") + Name;
                else
                {
                    if (Name != IP)
                        return (!char.IsDigit(Name[0]) ? "A" : "B") + Name;
                    else
                    {
                        var ver = Version.Parse(IP);
                        var str = string.Format("C{0:000}.{1:000}.{2:000}.{3:000}",
                            ver.Major, ver.Minor, ver.Build, ver.Revision);
                        return str;
                    }
                }
            }
        }

        private bool _scanCompleted = false;
        public bool ScanCompleted
        {
            get { return _scanCompleted; }
            private set { _scanCompleted = value; }
        }

        public ObservableCollection<FileResult> Files { get; set; }

        public int DistinctCount
        {
            get 
            {
                var items = Files.GroupBy(_ => _.Sha1).Select(_ => _.Key);
                return items.Count();
            }
        }

        public double TotalDistinctCount { get; set; }

        public double TotalCount { get; set; }

        public string StatusText { get; set; }

        private bool _canceling;
        public bool Canceling
        {
            get { return _canceling; }
            set 
            {
                if (value && !Completed)
                {
                    _canceling = value;
                    StatusText = "Scan operation was canceled.";
                }

                RaisePropertyChanged("StatusText");
                RaisePropertyChanged("Canceling");
                RaisePropertyChanged("InProgress");
                RaisePropertyChanged("Completed");
            }
        }

        public bool InProgress
        {
            get
            {
                return Canceling ? false : Files.Count < TotalCount;
            }
        }

        public bool Completed
        {
            get
            {
                return Canceling ? false : Files.Count >= TotalCount;
            }
        }

        public bool HasBadFiles { get; set; }

        public bool HasMalwares { get; set; }

        public bool HasUnknowns { get; set; }

        public bool HasPending { get; set; }

        public bool HasPendingFls { get; set; }

        public bool HasFailed { get; set; }

        public void MergeFile(FileResult file, string filePath)
        {
            if (!Files.Any(_ => _.Equals(file) && _.Path == filePath))
            {
                Files.Add(new FileResult(file) {Path =  filePath});
            }
            UpdateStatus();
        }

        public void UpdateFile(FileResult file)
        {
            foreach (var result in Files)
            {
                if (!result.Equals(file))
                    continue;

                result.UpdateValkyrieResult(file);
            }
            UpdateStatus();
        }

        public void UpdateFileIfPredicate(string sha1, ValkyrieVerdict verdict, Predicate<ValkyrieVerdict> predicate)
        {
            var needUpdate = false;
            foreach (var result in Files)
            {
                if (result.Sha1 != sha1)
                    continue;
                if (predicate(result.ValkyrieAnalysisResult.Verdict))
                {
                    result.UpdateValkyrieResult(verdict);
                    needUpdate = true;
                }
            }
            if (needUpdate)
                UpdateStatus();
        }

        public void UpdateStatus()
        {
            int malwareCount = 0, 
                unknownCount = 0, 
                failedCount = 0, 
                pendingCount = 0, 
                distinctCount = 0,
                pendingFls = 0,
                cleanCount = 0;

            for (int i = 0; i < Files.Count; i++)
            {
                var file = Files[i];
                if (file.IsMalware)
                    malwareCount++;
                else if (file.IsUnknown)
                    unknownCount++;
                else if (file.Failed)
                    failedCount++;
                else if (file.Pending)
                    pendingCount++;
                else if (file.PendingFls)
                    pendingFls++;
                else
                    cleanCount++;
            }

            if (Files.Count >= TotalCount)
            {
                StatusText =
                    $"Completed: Total files scanned: {Files.Count}. Unknown files: {unknownCount}. Malicious files: {malwareCount}.";
                
            }
            else if (TotalDistinctCount > 0)
            {
                distinctCount = DistinctCount;
                StatusText = string.Format("{0} files scanned ({3}). Unknown files: {1}. Malicious files: {2}.",
                    Files.Count, unknownCount, malwareCount, string.Format("{0:0.00}%",
                    (((distinctCount > TotalDistinctCount ? TotalDistinctCount : distinctCount) / TotalDistinctCount) * 100d)).Replace(".00", string.Empty));
            }

            if (cleanCount > 0)
                StatusText += $" Trusted/Clean files: {cleanCount}.";
            if (failedCount > 0)
                StatusText += $" Not analyzed: {failedCount}.";
            if (pendingCount > 0)
                StatusText += $" In Analysis files: {pendingCount}.";
            if (pendingFls > 0)
                StatusText += $" Lookup in Progress: {pendingFls}.";

            HasMalwares = malwareCount > 0;
            HasUnknowns = malwareCount == 0 && unknownCount > 0;
            HasFailed = malwareCount == 0 && unknownCount == 0 && failedCount > 0;
            HasPendingFls = malwareCount == 0 && unknownCount == 0 &&
                failedCount == 0 && pendingFls > 0;
            HasPending = malwareCount == 0 && unknownCount == 0 && 
                failedCount == 0 && pendingFls == 0 && pendingCount > 0;
            HasBadFiles = HasMalwares || HasUnknowns || HasFailed || HasPending || HasPendingFls;

            RaisePropertyChanged(nameof(HasBadFiles));
            RaisePropertyChanged(nameof(HasMalwares));
            RaisePropertyChanged(nameof(HasUnknowns));
            RaisePropertyChanged(nameof(HasPending));
            RaisePropertyChanged(nameof(HasPendingFls));
            RaisePropertyChanged(nameof(HasFailed));

            RaisePropertyChanged(nameof(StatusText));
            RaisePropertyChanged(nameof(InProgress));
            RaisePropertyChanged(nameof(Completed));
        }

        public bool Equals(ComputerInformation other)
        {
            return TargetUid == other.TargetUid && FilePath == other.FilePath;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
