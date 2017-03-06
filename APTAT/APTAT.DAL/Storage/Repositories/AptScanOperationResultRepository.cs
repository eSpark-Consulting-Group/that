using System;
using System.Data.Entity;
using System.Linq;
using APTAT.DAL;
using APTAT.DAL.Entities;
using APTAT.DAL.Storage.Core;
using System.Diagnostics;

using APTAT.DAL.Enums;
using APTAT.DAL.Storage.DTO;

namespace APTAT.Core.Storage.Repositories
{
    public interface IAptScanOperationResultRepository : IRepository<AptScanOperationResult>
    {
        AptScanOperationResult GetByOperationUid(Guid operationUid);
        IAptScanOperationDTO[] Search(string keyworld);
    }

    internal sealed class AptScanOperationResultRepository
        : Repository<AptEntities, AptScanOperationResult, AptScanOperationResult>, IAptScanOperationResultRepository
    {
        public AptScanOperationResultRepository(AptEntities dbContext)
            : base(dbContext)
        { }

        protected override DbSet<AptScanOperationResult> DbSet
        {
            get { return DbContext.AptScanOperaionResults; }
        }

        public override IQueryable<AptScanOperationResult> EntityQuery
        {
            get { return DbContext.AptScanOperaionResults; }
        }

        public AptScanOperationResult GetByOperationUid(Guid operationUid)
        {
            return DbSet.Where(o => o.OperationUid == operationUid).ExecuteDbQueryFirstOrDefault();
        }

        public IAptScanOperationDTO[] Search(string keyword)
        {
      //      DbContext.Database.Log = (x) => Debug.Write(x);
            var query = DbContext.AptScanOperaionResults;
            if (!String.IsNullOrEmpty(keyword))
            {
                query.Where(o => o.AptScanResults.Any(r => r.ComputerName.Contains(keyword) 
                    || r.Modules.Any(m => m.Name.Contains(keyword))));
            }

            var query2 = from i in query
                         let m = 
                         (from module in i.AptScanResults.SelectMany(sr=>sr.Modules)
                          join valkyrieFile in i.ValkyrieScanInfo.ValkyrieFileInfos
                          on new {sha1 =  module.Sha1, oId = module.AptScanResult.OperationResultId}
                          equals new {sha1 = valkyrieFile.Sha1, oId = valkyrieFile.ValkyrieScanInfoId}
                          into joined
                          from j in joined.DefaultIfEmpty()
                          select new {Module = module, Valkyrie = j})
                         select 
                         new AptScanOperationDTO()
                         {
                             OperationUid = i.OperationUid,
                             Started = i.Started,
                             Finished = i.Finished,
                             ScanMethod = i.ScanMethod,                             
                             DiscovererDescription = i.SourceDescription,                             
                             FilesCount = m.Count(),
                             UnknownsCount = m.Count(f1 => f1.Valkyrie != null &&
                                    f1.Valkyrie.Verdict == ValkyrieVerdict.Undetected
                                    && f1.Module.CloudVerdict != CloudVerdict.Malware),
                             MaliciousCount = m.Count(f1 => f1.Module.CloudVerdict == CloudVerdict.Malware
                                    || (f1.Valkyrie != null && f1.Valkyrie.Verdict == ValkyrieVerdict.Malware)),
                             TotalDevicesScanned = i.AptScanResults.Count(r2=>r2.Succeeded),
                             ValkyrieOperationInfo = i.ValkyrieScanInfo != null ?
                             new ValkyrieOperationInfo
                             {
                                 ApiKey = i.ValkyrieScanInfo.ApiKey, 
                                 ScanId = i.ValkyrieScanInfo.ScanId
                             } : null

                         };

            return query2.ExecuteDbQuery();
        }
    }
}
