using System;
using System.IO;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core.Reports
{
    public static class ScanModuleMapper
    {
        public static ScanModule Map(this IModule module)
        {
            if (module == null) throw new InvalidOperationException();
            ScanModule sm = new ScanModule();
            sm.Accessed = module.Accessed;
            sm.AccessedInternal = module.AccessedInternal;
            sm.Attributes = module.Attributes;
            sm.Cloud = module.Cloud;
            sm.CloudDate = module.CloudDate;
            sm.Copyright = module.Copyright;
            sm.Created = module.Created;
            sm.CreatedInternal = module.CreatedInternal;
            sm.Description = module.Description;
            sm.FileAge = module.FileAge;
            sm.FileAttributes = module.FileAttributes;
            sm.FileSizeFormatted = module.FileSizeFormatted;
            sm.Id = module.Id;
            sm.IsBad = module.IsBad;
            sm.IsUnknownOrFailed = module.IsUnknownOrFailed;
            sm.Modified = module.Modified;
            sm.ModifiedInternal = module.ModifiedInternal;
            sm.Path = module.Path;
            sm.Product = module.Product;
            sm.Sha1 = module.Sha1;
            sm.Signer = module.Signer;
            sm.Size = module.Size;
            sm.Version = module.Version;
            return sm;            
        }
    }
    public class ScanModule : IModule
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
    }
}