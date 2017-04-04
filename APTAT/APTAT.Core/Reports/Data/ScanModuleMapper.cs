using System;
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
}