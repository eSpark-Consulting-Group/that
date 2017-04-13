using System;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core.Reports
{
    public static class ScanModuleMapper
    {
        public static ScanModule Map(this IModule module)
        {
            if (module == null) throw new InvalidOperationException();
            ScanModule sm = new ScanModule
            {
                Accessed = module.Accessed,
                AccessedInternal = module.AccessedInternal,
                Attributes = module.Attributes,
                Cloud = module.Cloud,
                CloudDate = module.CloudDate,
                Copyright = module.Copyright,
                Created = module.Created,
                CreatedInternal = module.CreatedInternal,
                Description = module.Description,
                FileAge = module.FileAge,
                FileAttributes = module.FileAttributes,
                FileSizeFormatted = module.FileSizeFormatted,
                Id = module.Id,
                IsBad = module.IsBad,
                IsUnknownOrFailed = module.IsUnknownOrFailed,
                Modified = module.Modified,
                ModifiedInternal = module.ModifiedInternal,
                Path = module.Path,
                Product = module.Product,
                Sha1 = module.Sha1,
                Signer = module.Signer,
                Size = module.Size,
                Version = module.Version
            };
            return sm;            
        }
    }
}