using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using APTAT.Core.Descent.Apt;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;

namespace APTAT.Core.Reports
{
    public class DetailedBadFile : IModule
    {
        public string Sha1 { get; set; }
        public CloudVerdict Cloud { get; set; }
        public string Id { get; set; }
        public string Path { get; set; }
        public DateTime CloudDate { get; set; }
        public long Size { get; set; }
        public string Attributes { get; set; }
        public DateTime CreatedInternal { get; set; }
        public DateTime ModifiedInternal { get; set; }
        public DateTime AccessedInternal { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Accessed { get; set; }
        public string Signer { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Copyright { get; set; }
        public string Product { get; set; }
        public FileAttributes FileAttributes { get; set; }
        public string FileSizeFormatted { get; set; }
        public string FileAge { get; set; }
        public bool IsBad { get; set; }
        public bool IsUnknownOrFailed { get; set; }
        private readonly List<ScannedTarget> _targets = new List<ScannedTarget>();


        public DetailedBadFile(IModule module, ScannedTarget target)
        {
            if (module != null)
            {
                Sha1 = module.Sha1;
                Name = System.IO.Path.GetFileName(module.Path);
                Path = module.Path;
                Size = module.Size;
                Signer = module.Signer.TrimTo();
                Version = module.Version;
                Copyright = module.Copyright;
                Product = module.Product.TrimTo();
                Cloud = module.Cloud;
                FileSizeFormatted = module.FileSizeFormatted;
                Created = module.Created;
                Accessed = module.Accessed;
                Modified = module.Modified;
                FileAge = module.FileAge;
                target.BadFileInstances.Add(module.Path);
            }
            
            _targets.Add(target);
        }

        public string Name { get; set; }

        public virtual BadFileType FileType
        {
            get
            {

                if (Cloud == CloudVerdict.NoVerdict || Cloud == CloudVerdict.FailedToAnalyze)
                    return BadFileType.NotAnalyzed;

                return Cloud == CloudVerdict.Unknown || Cloud == CloudVerdict.FirstSeen ? BadFileType.Unknown : BadFileType.Infected;


            }
        }

        public int TargetsCount
        {
            get { return Targets.Count; }
        }

        public string CloudString
        {
            get
            {
                switch (Cloud)
                {
                    case CloudVerdict.Safe:
                        return "Trusted";
                    case CloudVerdict.Unknown:
                        return "Unknown";
                    case CloudVerdict.Malware:
                        return "Malware";
                    case CloudVerdict.FirstSeen:
                        return "Unknown (FirstSeen)";
                    case CloudVerdict.FailedToAnalyze:
                    case CloudVerdict.NoVerdict:
                        return "Lookup in Progress";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                ;
            }
        }


        public List<ScannedTarget> Targets => _targets;

        public int InstancesTotalCount
        {
            get
            {
                return _targets.Sum(t => t.BadFileInstances.Count);
            }
        }
    }
}