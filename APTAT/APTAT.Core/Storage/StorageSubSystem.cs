using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Core.Apt;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.DAL.Entities;
using APTAT.DAL.Storage;
using AptScanResult = APTAT.DAL.Entities.AptScanResult;

namespace APTAT.Core.Storage
{
    public class StorageSubSystem : IStorageSubSystem
    {
        private readonly IEntityStorage _entityStorage;
        public StorageSubSystem(IEntityStorage storage)
        {
            _entityStorage = storage;
        }
        public void SaveOperationContext(IAptScanOperationContext scanOperationContext)
        {
            var dataObject = new AptScanOperationResult
            {
                OperationUid = scanOperationContext.OperationUid,
                Started = scanOperationContext.Started,
                SourceDescription = scanOperationContext?.DevicesSourceDescription?.Description,
                Source = scanOperationContext?.DevicesSourceDescription.Type,

            };
            foreach (var aptScanResult in scanOperationContext.ScanResults)
            {
                dataObject.AptScanResults.Add(new AptScanResult
                {
                    IpAddress = aptScanResult.Computer?.IPAddress,
                    ComputerName = aptScanResult.Computer?.Name,
                    OrganizationUnit = aptScanResult.Computer?.OrganizationUnit,
                    OsName = aptScanResult.AptReport?.Computer?.Os.ToString(),
                    LdapPath = aptScanResult.Computer?.LdapPath,
                    TargetUid = aptScanResult.TargetUid,
                    Succeeded = aptScanResult.Succeeded,
                    StatusMessage = aptScanResult.ShortStatusMessage.ToString()
                });
            }
            using (var context = _entityStorage.GetContext())
            {
                context.AptScanOperationResultRepository.Add(dataObject);
                context.SaveChanges();
            }
        }
        public void UpdateScanResults(IAptScanOperationContext scanOperationContext)
        {
            using (var context = _entityStorage.GetContext())
            {
                var operation = context.AptScanOperationResultRepository.GetByOperationUid(scanOperationContext.OperationUid);
                operation.Finished = scanOperationContext.CompletedTime;
                context.SaveChanges();
            }
        }
        public void SaveScanResults(Guid operationUid, IAptScanResult scanResult)
        {
            using (var context = _entityStorage.GetContext())
            {
                var result = context.AptScanResultRepository.GetByTargetUidAndOperationUid(scanResult.Computer.Uid,
                    operationUid);
                result.Succeeded = scanResult.Succeeded;
                result.StatusMessage = scanResult.ShortStatusMessage.ToString();
                result.OrganizationUnit = scanResult.Computer.OrganizationUnit;                
                result.OsName = scanResult?.AptReport.Computer.Os.ToString();
                result.TargetUid = scanResult.TargetUid;

                foreach (var file in scanResult.Files)
                {
                    result.Modules.Add(Mapper.MapModule(file, VerdictSource.RemoteAgent));
                }
                context.SaveChanges();
            }
        }
    }
}
