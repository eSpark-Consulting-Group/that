using APTAT.Core.Descent.Apt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.DAL.Enums;
using System.IO;
using APTAT.DAL.Storage.DTO;
using APTAT.DAL.Entities;
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
            module.Id = Int32.Parse(file.Id);
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

        public static AptScanOperationDTO[] MapOperationToBusiness(AptScanOperationResult[] data)
        {
            List<AptScanOperationDTO> result = new List<AptScanOperationDTO>();
            foreach (var item in data)
            {
                result.Add(MapOperationToBusinessItem(item));
            }
            return result.ToArray();

        }
        public static AptScanOperationDTO MapOperationToBusinessItem(AptScanOperationResult operation)
        {
            AptScanOperationDTO dto = new AptScanOperationDTO();
            dto.SourceDescription = operation.SourceDescription;
            dto.OperationUid = operation.OperationUid;
            dto.Started = operation.Started;
            dto.TotalDevicesScanned = operation.AptScanResults.Count;
            //dto.
            //dto.UnknownsCount = operation.AptScanResults. 
            return dto;
        }
     

    }



   
}
