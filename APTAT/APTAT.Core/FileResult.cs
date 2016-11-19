using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ValkyrieVerdict = APTAT.Valkyrie.ValkyrieVerdict;

namespace APTAT.Core
{
    public class FileResult : IEquatable<FileResult>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(String name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public FileResult()
        {
            Computers = new ObservableCollection<ComputerInformation>();
        }

        public FileResult(Module badFile)
        {
            Computers = new ObservableCollection<ComputerInformation>();
            Id = badFile.Id;
            Path = badFile.Path;
            Cloud = badFile.Cloud;
            Size = badFile.Size;
            SizeSorting = SizeToSorting(badFile.Size);
            Attributes = badFile.Attributes;
            Created = badFile.Created;
            Modified = badFile.Modified;
            Accessed = badFile.Accessed;
            Sha1 = badFile.Sha1.Trim().ToLower();
            Signer = badFile.Signer;
            Description = badFile.Description;
            Version = badFile.Version;
            Copyright = badFile.Copyright;
            Product = badFile.Product;
        }

        public FileResult(FileResult result)
        {
            Computers = new ObservableCollection<ComputerInformation>();
            Id = result.Id;
            Path = result.Path;
            Cloud = result.Cloud;
            Size = result.Size;
            SizeSorting = result.SizeSorting;
            Attributes = result.Attributes;
            Created = result.Created;
            Modified = result.Modified;
            Accessed = result.Accessed;
            Sha1 = result.Sha1.Trim().ToLower();
            Signer = result.Signer;
            Description = result.Description;
            Version = result.Version;
            Copyright = result.Copyright;
            Product = result.Product;
            ValkyrieAnalysisResult = result.ValkyrieAnalysisResult;
            ScanCompleted = false;
        }

        public string Id { get; set; }
        public string Path { get; set; }
        public Descent.Apt.CloudVerdict Cloud { get; set; }
        public DateTime CloudDate { get; set; }
        public long Size { get; set; }
        public string Attributes { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Accessed { get; set; }
        public string Sha1 { get; set; }
        public string Signer { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Copyright { get; set; }
        public string Product { get; set; }

        public string FullName
        {
            get
            {
                if(string.IsNullOrEmpty(this.Product))
                {
                    return this.Name;
                }

                return String.Format("{0} ({1})", this.Name, this.Product);
            }
        }        

        public bool IsMalware
        {
            get
            {
                if (this.ValkyrieAnalysisResult != null)
                {

                    return ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.Malware || ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.PotentiallyUnwantedApplication;
                }
                return false;
            }
        }

        public string MalwareProbability
        {
            get
            {
                if (this.ValkyrieAnalysisResult != null && ValkyrieAnalysisResult.MalwareProbability.HasValue)
                {                    
                    return ValkyrieAnalysisResult.MalwareProbability.Value.ToString("P", CultureInfo.InvariantCulture);
                }
                return String.Empty;
            }
        }

        public bool IsUnknown
        {
            get
            {
                return ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.Undetected;
            }

        }

        public bool Clean
        {
            get
            {
                return ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.Clean;
            }
        }

        public bool Failed
        {
            get
            {
                return ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.NotAnalyzed;
            }
        }

        public bool Pending
        {
            get
            {
                return ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.Pending || ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.FirstSeen;
            }
        }

        public bool PendingFls
        {
            get
            {
                return ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.PendingFls;
            }
        }

        public string Name
        {
            get
            {
                return System.IO.Path.GetFileName(this.Path);
            }
        }
        public FileAttributes FileAttributes
        {
            get
            {
                Int32 flags = 0;
                try
                {
                    flags = Convert.ToInt32(Attributes ?? "0", 16);
                }
                catch (Exception)
                {
                }

                return (FileAttributes)flags;
            }
        }

        public string FileSizeFormatted
        {
            get { return SizeToString(Size); }
        }  
        
        public string SizeSorting
        {
            get;set;
        }

        private static string SizeToSorting(long len)
        {
            string[] sizes = { "A", "B", "C", "D" };

            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }

            string result = String.Format("{1}{0:0.000}", len / 1000d, sizes[order]);

            return result;
        }

        private static string SizeToString(long len)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }
         
            string result = String.Format("{0:0.##} {1}", len, sizes[order]);

            return result;            
        }

        public string FileAge
        {
            get { return FileAgeFormatter.GetAge(Created); }
        }

        private bool _scanCompleted = true;
        public bool ScanCompleted
        {
            get { return _scanCompleted; }
            private set { _scanCompleted = value; }
        }

        public ValkyrieAnalysisResult ValkyrieAnalysisResult { get; set; }

        public string VerdictString { get { return ValkyrieAnalysisResult.VerdictString; } }

        public void UpdateValkyrieResult(ValkyrieVerdict verdict)
        {
            ValkyrieAnalysisResult.Verdict = verdict;

            RaisePropertyChanged(nameof(VerdictString));
            RaisePropertyChanged(nameof(ValkyrieAnalysisResult));
            RaisePropertyChanged(nameof(IsMalware));
            RaisePropertyChanged(nameof(IsUnknown));
            RaisePropertyChanged(nameof(Clean));
            RaisePropertyChanged(nameof(Failed));
            RaisePropertyChanged(nameof(Pending));
            RaisePropertyChanged(nameof(PendingFls));
        }

        public void UpdateValkyrieResult(FileResult result)
        {
            ValkyrieAnalysisResult = result.ValkyrieAnalysisResult;

            RaisePropertyChanged(nameof(VerdictString));
            RaisePropertyChanged(nameof(ValkyrieAnalysisResult));
            RaisePropertyChanged(nameof(IsMalware));
            RaisePropertyChanged(nameof(IsUnknown));
            RaisePropertyChanged(nameof(Clean));
            RaisePropertyChanged(nameof(Failed));
            RaisePropertyChanged(nameof(Pending));
            RaisePropertyChanged(nameof(PendingFls));
        }

        public ObservableCollection<ComputerInformation> Computers { get; set; }

        public string InstancesCount
        {
            get 
            {
                if (Computers == null || !Computers.Any())
                    return string.Empty;

                if (Computers.Count == 1)
                    return "1 instance.";

                var uniqueComputers = Computers.GroupBy(_ => _.TargetUid).Select(_ => _.Key).Count();

                return string.Format("{0} instances on {1} computer{2}.", Computers.Count, 
                    uniqueComputers, uniqueComputers > 1 ? "s" : string.Empty);
            }
        }

        public void MergeComputers(IEnumerable<ComputerInformation> computers, ref int count)
        {
            foreach (var computer in computers)
            {
                if (!Computers.Any(_ => _.Equals(computer)))
                {
                    Computers.Add(computer);
                    count++;
                }
            }

            RaisePropertyChanged("InstancesCount");
        }

        public override string ToString()
        {
            return Path;
        }

        public bool Equals(FileResult other)
        {
            return Sha1 == other.Sha1 && ValkyrieAnalysisResult != null && other.ValkyrieAnalysisResult != null;
        } 
    }
}
