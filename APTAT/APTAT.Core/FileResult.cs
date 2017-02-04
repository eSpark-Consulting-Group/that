using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;
using ValkyrieVerdict = APTAT.Valkyrie.ValkyrieVerdict;

namespace APTAT.Core
{
    public class FileResult : IEquatable<FileResult>
    {
        private ValkyrieAnalysisResult _valkyrieAnalysisResult;
        public FileResult(IModule badFile)
        {
            Computers = new List<ComputerInformation>();
            Id = badFile.Id;
            Path = badFile.Path;
            Cloud = badFile.Cloud;
            Size = badFile.Size;
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
            Computers = new List<ComputerInformation>();
            Id = result.Id;
            Path = result.Path;
            Cloud = result.Cloud;
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
            ValkyrieAnalysisResult = new ValkyrieAnalysisResult(result.ValkyrieAnalysisResult);
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
        public string Sha1 { get; }
        public string Signer { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Copyright { get; set; }
        public string Product { get; set; }

        public string FullName
        {
            get
            {
                return string.IsNullOrEmpty(this.Product) ? this.Name : $"{this.Name} ({this.Product})";
            }
        }        

        public bool IsMalware
        {
            get {
                return ValkyrieAnalysisResult != null &&
                       (ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.Malware ||
                        ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.PotentiallyUnwantedApplication);
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
        

        public string FileAge
        {
            get { return FileAgeFormatter.GetAge(Created); }
        }

        public ValkyrieAnalysisResult ValkyrieAnalysisResult
        {
            get { return _valkyrieAnalysisResult; }
            private set
            {
                if (_valkyrieAnalysisResult == value) return;;
                _valkyrieAnalysisResult = value;
                VerdictModified = DateTime.UtcNow;
            }
        }

        public DateTime VerdictModified { get; private set; }
        
        internal void SetValkyrieResult(ValkyrieAnalysisResult valkyrieAnalysisResult)
        {
            ValkyrieAnalysisResult = valkyrieAnalysisResult;
        }
        
        public List<ComputerInformation> Computers { get; set; }
        
        public bool Equals(FileResult other)
        {
            return Sha1 == other.Sha1 && ValkyrieAnalysisResult != null && other.ValkyrieAnalysisResult != null;
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Sha1);
        }

        public override bool Equals(object obj)
        {
            var other = obj as FileResult;
            return other != null && Equals(other);
        }
    }
}
