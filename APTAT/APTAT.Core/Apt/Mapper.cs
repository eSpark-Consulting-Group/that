using APTAT.Core.Descent.Apt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.DAL.Entities;

namespace APTAT.Core.Apt
{
    public static class Mapper
    {
        public static DAL.Entities.Module MapModule(IModule file, VerdictSource verdictSource)
        {
            var module = new DAL.Entities.Module();
            module.Accessed = file.AccessedInternal;            
            module.Attributes = file.Attributes;            
            module.CloudDate = file.CloudDate;
            module.CloudVerdict = MapCloud(file.Cloud);
            module.Copyright = file.Copyright;
            module.Created = file.CreatedInternal;            
            module.Description = file.Description;
            module.FileAge = file.FileAge;
            module.FileAttributes = file.FileAttributes;
            module.FileSizeFormatted = file.FileSizeFormatted;
            module.Id = Int32.Parse(file.Id);
            module.IsBad = file.IsBad;
            module.IsUnknownOrFailed = file.IsUnknownOrFailed;
            module.Modified = file.ModifiedInternal;            
            module.Path = file.Path;
            module.Product = file.Product;
            module.Sha1 = file.Sha1;
            module.Signer = file.Signer;
            module.Size = file.Size;
            module.VerdictSource = verdictSource;
            module.Version = file.Version;            
            return module;

        }

        public static DAL.Entities.CloudVerdict MapCloud(Descent.Apt.CloudVerdict cloudVerdict)
        {
            switch (cloudVerdict)
            {
                case Descent.Apt.CloudVerdict.Safe:
                    return DAL.Entities.CloudVerdict.Safe;
                    
                case Descent.Apt.CloudVerdict.Unknown:
                    return DAL.Entities.CloudVerdict.Unknown;
                    
                case Descent.Apt.CloudVerdict.Malware:
                    return DAL.Entities.CloudVerdict.Malware;
                    
                case Descent.Apt.CloudVerdict.FirstSeen:
                    return DAL.Entities.CloudVerdict.FirstSeen;
                    
                case Descent.Apt.CloudVerdict.FailedToAnalyze:
                    return DAL.Entities.CloudVerdict.FailedToAnalyze;
                    
                case Descent.Apt.CloudVerdict.NoVerdict:
                    return DAL.Entities.CloudVerdict.NoVerdict;
                                        
                default:
                    throw new ArgumentOutOfRangeException(nameof(cloudVerdict));
                    
            }
        }
    }
}
