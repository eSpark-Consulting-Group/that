using System;
using System.Collections.ObjectModel;
using System.Linq;
using APTAT.Core;
using APTAT.Valkyrie;
using OperatingSystem = APTAT.Core.Descent.Apt.OperatingSystem;

namespace FAT.Application.Models.Valkyrie
{
    public class ValkyrieComputer : DomainObject, IEquatable<ValkyrieComputer>
    {
        public ValkyrieComputer()
        {
            Files = new ObservableCollection<ValkyrieFile>();
            ScanCompleted = false;
        }

        public ValkyrieComputer(ValkyrieComputer computer)
        {
            Files = new ObservableCollection<ValkyrieFile>();
            ScanCompleted = true;
            Name = computer.Name;
            Sid = computer.Sid;
            Os = computer.Os;
            TargetUid = computer.TargetUid;
            IP = computer.IP;

        }

        public ValkyrieComputer(ComputerInformation computer)
        {
            Files = new ObservableCollection<ValkyrieFile>();
            Name = computer.Name;
            Sid = computer.Sid;
            Os = computer.Os;
            TargetUid = computer.TargetUid;
            FilePath = computer.FilePath;
            IP = computer.IP;
        }

        public bool ScanCompleted { get; private set; }
        public string Name { get; set; }
        public string Sid { get; set; }
        public OperatingSystem Os { get; set; }
        public string IP { get; set; }
        public string FilePath { get; set; }
        public Guid TargetUid { get; set; }
        public string FullName => $"{Name} ({IP})";
        public bool HasBadFiles { get; set; }
        public ObservableCollection<ValkyrieFile> Files { get; set; }

        public bool HasMalwares { get; set; }
        public bool HasUnknowns { get; set; }
        public bool HasPending { get; set; }
        public bool HasPendingFls { get; set; }
        public bool HasFailed { get; set; }
        private int MalwareCount { get; set; }
        private int UnknownCount { get; set; }
        private int FailedCount { get; set; }
        private int PendingCount { get; set; }
        private int PendingFls { get; set; }
        private int CleanCount { get; set; }
        public double TotalDistinctCount { get; set; }
        public double TotalCount { get; set; }

        public int DistinctCount
        {
            get
            {
                var items = Files.GroupBy(_ => _.Sha1).Select(_ => _.Key);
                return items.Count();
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

        private void UpdateVerdictForFile(ValkyrieFile result, Func<ValkyrieFile, bool> updateAction)
        {
            var previousVerdict = result.ValkyrieVerdict;
            if (updateAction(result))
            {
                switch (previousVerdict)
                {
                    case ValkyrieVerdict.Clean:
                        CleanCount--;
                        break;
                    case ValkyrieVerdict.Pending:
                    case ValkyrieVerdict.FirstSeen:
                        PendingCount--;
                        break;
                    case ValkyrieVerdict.NotAnalyzed:
                        FailedCount--;
                        break;
                    case ValkyrieVerdict.PendingFls:
                        PendingFls--;
                        break;
                    case ValkyrieVerdict.Malware:
                    case ValkyrieVerdict.PotentiallyUnwantedApplication:
                        MalwareCount--;
                        break;
                    case ValkyrieVerdict.Undetected:
                        UnknownCount--;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("ValkyrieVerdict");
                }
                if (result.IsMalware)
                    MalwareCount++;
                else if (result.Failed)
                    FailedCount++;
                else if (result.IsUnknown)
                    UnknownCount++;
                else if (result.Pending)
                    PendingCount++;
                else if (result.PendingFls)
                    PendingFls++;
                else if (result.Clean)
                    CleanCount++;
                else
                    throw new ArgumentOutOfRangeException("ValkyrieVerdict");
            }
        }

        public string StatusText
        {
            get
            {
                if (Canceling && !Completed) return "Scan operation was canceled.";
                string status = String.Empty;
                if (Files.Count >= TotalCount)
                {
                    status = $"Completed: Total files scanned: {Files.Count}. Unknown files: {UnknownCount}. Malicious files" +
                             $": {MalwareCount}.";
                }
                else if (TotalDistinctCount > 0)
                {
                    status = $"{Files.Count} files scanned ({$"{(((DistinctCount > TotalDistinctCount ? TotalDistinctCount : DistinctCount) / TotalDistinctCount) * 100d):0.00}%".Replace(".00", string.Empty)}). Unknown files: {UnknownCount}. Malicious files: {MalwareCount}.";
                }

                if (CleanCount > 0)
                    status += $" Trusted/Clean files: {CleanCount}.";
                if (FailedCount > 0)
                    status += $" Not analyzed: {FailedCount}.";
                if (PendingCount > 0)
                    status += $" In Analysis files: {PendingCount}.";
                if (PendingFls > 0)
                    status += $" Lookup in Progress: {PendingFls}.";
                return status;
            }
        }

        private bool _canceling;
        public bool Canceling
        {
            get { return _canceling; }
            set
            {
                if (value && !Completed)
                {
                    _canceling = value;
                }

                RaisePropertyChanged(nameof(StatusText));
                RaisePropertyChanged(nameof(Canceling));
                RaisePropertyChanged(nameof(InProgress));
                RaisePropertyChanged(nameof(Completed));
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

        public void MergeFile(ValkyrieFile file, string filePath)
        {
            lock (Files)
            {
                if (!Files.Any(_ => _.Equals(file) && _.Path == filePath))
                {
                    Files.Add(new ValkyrieFile(file) { Path = filePath });
                    if (file.IsMalware)
                        MalwareCount++;
                    else if (file.Failed)
                        FailedCount++;
                    else if (file.IsUnknown)
                        UnknownCount++;
                    else if (file.Pending)
                        PendingCount++;
                    else if (file.PendingFls)
                        PendingFls++;
                    else
                        CleanCount++;
                }
                UpdateStatus();
            }
        }

        public void UpdateFile(ValkyrieFile file)
        {
            lock (Files)
            {
                foreach (var result in Files)
                {
                    if (!result.Equals(file))
                        continue;

                    UpdateVerdictForFile(result, f => f.UpdateValkyrieResult(file));
                }
                UpdateStatus();
            }
        }

        public void UpdateFileIfPredicate(string sha1, ValkyrieAnalysisResult valkyrieAnalysisResult
            , Predicate<ValkyrieVerdict> predicate)
        {
            lock (Files)
            {
                var needUpdate = false;
                foreach (var result in Files)
                {
                    if (!String.Equals(result.Sha1, sha1, StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (predicate(result.ValkyrieVerdict))
                    {
                        UpdateVerdictForFile(result, f => f.UpdateValkyrieResult(valkyrieAnalysisResult));
                        needUpdate = true;
                    }
                }
                if (needUpdate)
                    UpdateStatus();
            }
        }

        public void UpdateStatus()
        {
            HasMalwares = MalwareCount > 0;
            HasUnknowns = MalwareCount == 0 && UnknownCount > 0;
            HasFailed = MalwareCount == 0 && UnknownCount == 0 && FailedCount > 0;
            HasPendingFls = MalwareCount == 0 && UnknownCount == 0 &&
                FailedCount == 0 && PendingFls > 0;
            HasPending = MalwareCount == 0 && UnknownCount == 0 &&
                FailedCount == 0 && PendingFls == 0 && PendingCount > 0;
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

        public bool Equals(ValkyrieComputer other)
        {
            return TargetUid == other.TargetUid && FilePath == other.FilePath;
        }
    }
}
