using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using APTAT.Core.Descent.Apt;
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

        public bool HasFailed { get; set; }

        public void MergeFile(FileResult file)
        {
            if (!Files.Any(_ => _.Equals(file) && _.Path == file.Path))
            {
                Files.Add(new FileResult(file));
                UpdateStatus();
            }
        }

        public void UpdateFile(FileResult file)
        {
            foreach (var result in Files)
            {
                if (!result.Equals(file))
                    continue;

                result.UpdateValkyrieResult(file);
                UpdateStatus();
            }
        }

        private void UpdateStatus()
        {
            if (TotalDistinctCount == 0)
                return;

            int malwareCount = 0, unknownCount = 0, failedCount = 0, pendingCount = 0, distinctCount = 0;

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
            }

            if (Files.Count >= TotalCount)
                StatusText = string.Format("Completed: Total programs scanned: {0}. Unknown programs found: {1}. Malwares found: {2}.",
                    Files.Count, unknownCount, malwareCount);
            else
            {
                distinctCount = DistinctCount;
                StatusText = string.Format("{0} programs scanned ({3}). Unknown programs found: {1}. Malwares found: {2}.",
                    Files.Count, unknownCount, malwareCount, string.Format("{0:0.00}%",
                    (((distinctCount > TotalDistinctCount ? TotalDistinctCount : distinctCount) / TotalDistinctCount) * 100d)).Replace(".00", string.Empty));
            }

            if (failedCount > 0)
                StatusText += string.Format(" Not analysed: {0}.", failedCount);

            HasBadFiles = malwareCount > 0 || unknownCount > 0 || failedCount > 0 || pendingCount > 0;
            HasMalwares = malwareCount > 0;
            HasUnknowns = malwareCount == 0 && unknownCount > 0;
            HasFailed = malwareCount == 0 && unknownCount == 0 && failedCount > 0;
            HasPending = malwareCount == 0 && unknownCount == 0 && failedCount == 0 && pendingCount > 0;

            RaisePropertyChanged("HasBadFiles");
            RaisePropertyChanged("HasMalwares");
            RaisePropertyChanged("HasUnknowns");
            RaisePropertyChanged("HasPending");
            RaisePropertyChanged("HasFailed");

            RaisePropertyChanged("StatusText");
            RaisePropertyChanged("InProgress");
            RaisePropertyChanged("Completed");
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
