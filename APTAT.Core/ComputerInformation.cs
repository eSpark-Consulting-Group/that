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
        
        public ComputerInformation(Computer computer)
        {
            Files = new ObservableCollection<FileResult>();
            Os = computer.Os;
            Name = computer.Name;
            Sid = computer.Sid;
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
        }

        public string Name { get; set; }
        public string Sid { get; set; }
        public OperatingSystem Os { get; set; }
        public string IP { get; set; }
        public string FilePath { get; set; }

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

        public double Count { get; set; }

        public string StatusText { get; set; }

        public bool Canceled { get; set; }

        public bool InProgress
        {
            get
            {
                return Canceled ? false : Files.Count < Count;
            }
        }

        public bool Completed
        {
            get
            {
                return Files.Count == Count;
            }
        }

        public void MergeFiles(FileResult file)
        {
            if (!Files.Any(_ => _.Equals(file)))
            {
                Files.Add(new FileResult(file));

                var malwareCount = Files.Count(_ => _.IsMalware);
                var unknownCount = Files.Count(_ => _.IsUnknown);

                if (Files.Count == Count)
                    StatusText = string.Format("Completed. Total programs scanned: {0}. Unknown programs found: {1}. Malwares found: {2}.",
                        Files.Count, unknownCount, malwareCount);
                else
                    StatusText = string.Format("{0} programs scanned ({3}). Unknown programs found: {1}. Malwares found: {2}.",
                        Files.Count, unknownCount, malwareCount, string.Format("{0:0.00}%", ((Files.Count / Count) * 100d)).Replace(".00", string.Empty));

                if (Completed)
                    Canceled = false;

                if (Canceled)
                    StatusText = "Scan operation was canceled.";

                RaisePropertyChanged("StatusText");
                RaisePropertyChanged("Canceled");
                RaisePropertyChanged("InProgress");
                RaisePropertyChanged("Completed");
            }
        }

        public bool Equals(ComputerInformation other)
        {
            return FullName == other.FullName && FilePath == other.FilePath;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
