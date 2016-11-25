using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using APTAT.Core;
using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;

namespace APTAT.Application.Models.Valkyrie
{
    public class ValkyrieFile : DomainObject, IEquatable<ValkyrieFile>
    {
        private bool _scanCompleted = true;
        private ValkyrieVerdict _valkyrieVerdict;

        public ObservableCollection<ValkyrieComputer> Computers { get; set; }
        public string Id { get; set; }
        public string Path { get; set; }
        public CloudVerdict FlsVerdict { get; set; }
        public DateTime CloudDate { get; set; }
        public long Size { get; set; }
        public string Attributes { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Accessed { get; set; }
        public bool IsWhiteListed { get; private set; }
        public string Sha1 { get; set; }
        public string Signer { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Copyright { get; set; }
        public string Product { get; set; }
        public string OperationStatus { get; private set; }
        public float? MalwareProbabilityValue { get; set; }
        public string SkipReason { get; set; }

        public ValkyrieVerdict ValkyrieVerdict
        {
            get { return _valkyrieVerdict; }
            set
            {
                _valkyrieVerdict = value; 
                RaisePropertyChanged(nameof(VerdictString));
                RaisePropertyChanged(nameof(ValkyrieVerdict));
                RaisePropertyChanged(nameof(IsMalware));
                RaisePropertyChanged(nameof(IsUnknown));
                RaisePropertyChanged(nameof(Clean));
                RaisePropertyChanged(nameof(Failed));
                RaisePropertyChanged(nameof(Pending));
                RaisePropertyChanged(nameof(PendingFls));
            }
        }

        public string FullName
        {
            get
            {
                return string.IsNullOrEmpty(this.Product) ? this.Name : $"{this.Name} ({this.Product})";
            }
        }

        public bool IsMalware
        {
            get
            {
                return (ValkyrieVerdict == ValkyrieVerdict.Malware ||
                        ValkyrieVerdict == ValkyrieVerdict.PotentiallyUnwantedApplication);
            }
        }

        public string MalwareProbability
        {
            get
            {
                if (MalwareProbabilityValue.HasValue)
                {
                    return MalwareProbabilityValue.Value.ToString("P", CultureInfo.InvariantCulture);
                }
                return String.Empty;
            }
        }

        public bool IsUnknown
        {
            get
            {
                return ValkyrieVerdict == ValkyrieVerdict.Undetected;
            }

        }

        public bool Clean
        {
            get
            {
                return ValkyrieVerdict == ValkyrieVerdict.Clean;
            }
        }

        public bool Failed
        {
            get
            {
                return ValkyrieVerdict == ValkyrieVerdict.NotAnalyzed;
            }
        }

        public bool Pending
        {
            get
            {
                return ValkyrieVerdict == ValkyrieVerdict.Pending || ValkyrieVerdict == ValkyrieVerdict.FirstSeen;
            }
        }

        public bool PendingFls
        {
            get
            {
                return ValkyrieVerdict == ValkyrieVerdict.PendingFls;
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
            get; set;
        }

        public ValkyrieFile()
        {
            Computers = new ObservableCollection<ValkyrieComputer>();
        }

        public ValkyrieFile(ValkyrieFile result)
        {
            Computers = new ObservableCollection<ValkyrieComputer>();
            ValkyrieVerdict = result.ValkyrieVerdict;
            Id = result.Id;
            Path = result.Path;
            FlsVerdict = result.FlsVerdict;
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
            OperationStatus = result.OperationStatus;
            SkipReason = result.SkipReason;
            MalwareProbabilityValue = result.MalwareProbabilityValue;
            ScanCompleted = false;
        }

        public ValkyrieFile(FileResult result)
        {
            Computers = new ObservableCollection<ValkyrieComputer>();
            Id = result.Id;
            Path = result.Path;
            FlsVerdict = result.Cloud;
            Size = result.Size;
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
            ValkyrieVerdict = result.ValkyrieAnalysisResult.Verdict;
            OperationStatus = result.ValkyrieAnalysisResult.OperationStatus;
            SkipReason = result.ValkyrieAnalysisResult.SkipReason;
            MalwareProbabilityValue = result.ValkyrieAnalysisResult.MalwareProbability;
            ScanCompleted = true;
            foreach (var computer in result.Computers)
            {
                Computers.Add(new ValkyrieComputer(computer));
            }
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

        public bool ScanCompleted
        {
            get { return _scanCompleted; }
            private set { _scanCompleted = value; }
        }

        public string VerdictString
        {
            get
            {
                switch (ValkyrieVerdict)
                {
                    case ValkyrieVerdict.NotAnalyzed:
                        return $"Not Analyzed ({OperationStatus.Replace(Environment.NewLine, "")})";
                    case ValkyrieVerdict.PotentiallyUnwantedApplication:
                        return "Potentially Unwanted";
                    case ValkyrieVerdict.Pending:
                    case ValkyrieVerdict.FirstSeen:
                        return "In Analysis";
                    case ValkyrieVerdict.Undetected:
                        return "No Threat Found";
                    case ValkyrieVerdict.PendingFls:
                        return "Lookup in Progress";
                }
                return ValkyrieVerdict.ToString();
            }
        }

        public bool UpdateValkyrieResult(ValkyrieAnalysisResult valkyrieAnalysisResult)
        {
            bool returnValue = ValkyrieVerdict != valkyrieAnalysisResult.Verdict;

            MalwareProbabilityValue = valkyrieAnalysisResult.MalwareProbability;
            ValkyrieVerdict = valkyrieAnalysisResult.Verdict;
            SkipReason = valkyrieAnalysisResult.SkipReason;
            OperationStatus = valkyrieAnalysisResult.OperationStatus;
            
            return returnValue;
        }
        public bool UpdateValkyrieResult(ValkyrieFile result)
        {
            bool returnValue = ValkyrieVerdict != result.ValkyrieVerdict;

            MalwareProbabilityValue = result.MalwareProbabilityValue;
            ValkyrieVerdict = result.ValkyrieVerdict;
            SkipReason = result.SkipReason;
            OperationStatus = result.OperationStatus;
            return returnValue;
        }

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

        public void MergeComputers(IEnumerable<ValkyrieComputer> computers, ref int count)
        {
            foreach (var computer in computers)
            {
                if (!Computers.Any(_ => _.Equals(computer)))
                {
                    Computers.Add(computer);
                    Interlocked.Increment(ref count);
                }
            }

            RaisePropertyChanged("InstancesCount");
        }

        public override string ToString()
        {
            return Path;
        }
        public bool Equals(ValkyrieFile other)
        {
            return String.Equals(Sha1, other.Sha1, StringComparison.OrdinalIgnoreCase);
        }
    }
}
