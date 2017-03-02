using APTAT.Core.Apt;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Storage.Repositories;
using APTAT.DAL.Storage;
using APTAT.DAL.Storage.Core;
using FAT.Application.Models;
using FAT.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAT.Application.Services
{
    public class DataService 
    {
        private IEntityStorage _storageContext;

        public DataService(IEntityStorage storageContext)
        {
            _storageContext = storageContext;
        }

         //<summary>
         
         //</summary>
         //<param name = "keyword" > Search by computer name and file name</param>
         //<returns></returns>
        public AptScanOperationDTO [] GetPreviousScans(string keyword)
        {
            using (var storage = _storageContext.GetContext())
            {
                var data = storage.AptScanOperationResultRepository.Search(keyword);
                return Mapper.MapOperationToBusiness(data);
            }
            
        }
        public IScanResultItem[] GetAptScanResults(Guid operationUid)
        {
            using (var storage = _storageContext.GetContext())
            {                
                var data =  storage.AptScanResultRepository.GetByOperationUid(operationUid);
                return data.MapCollection();
            }

        }

    }
}
