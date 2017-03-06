using APTAT.Core.Descent.Apt;
using System;
using APTAT.DAL.Enums;
using System.IO;
using APTAT.Core.Storage;
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
            module.Name = Path.GetFileName(file.Path);
            module.CloudVerdict = file.Cloud.ToData();
            module.Copyright = file.Copyright;
            module.Created = file.CreatedInternal;
            module.Description = file.Description;
            module.FileAge = file.FileAge;
            module.FileAttributes = file.FileAttributes;
            module.FileSizeFormatted = file.FileSizeFormatted;
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

        public static ETargetsDiscovererType ToDal(this APTAT.DeploymentCore.Directory.ETargetsDiscovererType source)
        {
            switch (source)
            {
                case APTAT.DeploymentCore.Directory.ETargetsDiscovererType.ActiveDirectory:
                    return ETargetsDiscovererType.ActiveDirectory;

                case APTAT.DeploymentCore.Directory.ETargetsDiscovererType.NetworkAddresses:
                    return ETargetsDiscovererType.NetworkAddresses;
                case APTAT.DeploymentCore.Directory.ETargetsDiscovererType.Workgroup:
                    return ETargetsDiscovererType.Workgroup;

                default:
                    throw new ArgumentOutOfRangeException(nameof(DeploymentCore.Directory.ETargetsDiscovererType));
            }
        }

    }
}
