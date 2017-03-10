using APTAT.Core.Storage.Repositories;
using APTAT.DAL.Storage;
using APTAT.DAL.Storage.DTO;
using FAT.Application.Models;
using System;
using System.Linq;
using FAT.Application.Models.Apt;

namespace FAT.Application.Services
{
    public class DataService : IDataService
    {
        private IEntityStorage _storageContext;

        public DataService(IEntityStorage storageContext)
        {
            _storageContext = storageContext;
        }


        public string GetEmails()
        {
            string emails = "";
            using (var context = _storageContext.GetContext())
            {
                var addresses = context.EmailRepository.EntityQuery.Select(e => e.Address);
                emails = String.Join(",", addresses);
                return emails;
            }
        }

        //<summary>

        //</summary>
        //<param name = "keyword" > Search by computer name and file name</param>
        //<returns></returns>
        public IScanOperation[] GetPreviousScans(string keyword)
        {
            IAptScanOperationDTO[] data;
            using (var storage = _storageContext.GetContext())
            {
                data = storage.AptScanOperationResultRepository.Search(keyword);
            }

            return data.MapCollection();

        }
        public IFatScanComputerResult[] GetScanOperationDetailed(Guid operationUid)
        {
            AptScanResultDTO[] data;
            using (var storage = _storageContext.GetContext())
            {                
                 data =  storage.AptScanResultRepository.GetByOperationUid(operationUid);
            }

            return data.MapCollection();
        }

    }
}
