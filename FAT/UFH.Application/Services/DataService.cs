
using APTAT.Core.Storage.Repositories;
using APTAT.DAL.Storage;
using APTAT.DAL.Storage.DTO;
using UFH.Application.Models;
using System;
using System.Linq;
using UFH.Application.Models.Apt;

namespace UFH.Application.Services
{
    public class DataService : IDataService
    {
        private Lazy<IEntityStorage> _storageContext;

        public DataService(Lazy<IEntityStorage> storageContext)
        {
            _storageContext = storageContext;
        }

        public string GetEmails()
        {
            string emails = "";
            using (var context = _storageContext.Value.Context)
            {
                var addresses = context.EmailRepository.EntityQuery.Select(e => e.Address);
                emails = String.Join(",", addresses);
                return emails;
            }
        }

        public bool HasPreviousScans()
        {
            int count;
            using (var storage = _storageContext.Value.Context)
            {
                count = storage.AptScanOperationResultRepository.GetCount();
            }
            return count > 0;
        }

        public IScanOperation[] GetPreviousScans(string keyword)
        {
            IAptScanOperationDTO[] data;
            using (var storage = _storageContext.Value.Context)
            {
                data = storage.AptScanOperationResultRepository.Search(keyword);
            }

            return data.MapCollection(_ => new ScanOperationResult(_, GetScanOperationDetailed(_.OperationUid)));

        }
        public IFatScanComputerResult[] GetScanOperationDetailed(Guid operationUid)
        {
            AptScanResultDTO[] data;
            using (var storage = _storageContext.Value.Context)
            {                
                 data =  storage.AptScanResultRepository.GetByOperationUid(operationUid);
            }

            return data.MapCollection();
        }

    }
}
