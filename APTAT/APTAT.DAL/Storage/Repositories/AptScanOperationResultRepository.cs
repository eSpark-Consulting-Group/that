using System;
using System.Data.Entity;
using System.Linq;
using APTAT.DAL;
using APTAT.DAL.Entities;
using APTAT.DAL.Storage.Core;
using System.Diagnostics;

using APTAT.DAL.Enums;

namespace APTAT.Core.Storage.Repositories
{
    public interface IAptScanOperationResultRepository : IRepository<AptScanOperationResult>
    {
        AptScanOperationResult GetByOperationUid(Guid operationUid);
        AptScanOperationDTO[] Search(string keyworld);
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

        public AptScanOperationDTO[] Search(string keyword)
        {
      //      DbContext.Database.Log = (x) => Debug.Write(x);
            var query = DbContext.AptScanOperaionResults;
            if (keyword != null)
            {
                query.Where(o => o.AptScanResults.Any(r => r.ComputerName.Contains(keyword) || r.Modules.Any(m => m.Name.Contains(keyword))));
            }

            var query2 = from i in query
                         let m = i.AptScanResults.SelectMany(sr=>sr.Modules)
                         select 
                         new AptScanOperationDTO()
                         {
                             OperationUid = i.OperationUid,
                             Started = i.Started,
                             Finished = i.Finished,
                             ScanMethod = i.ScanMethod,                             
                             DiscovererDescription = i.SourceDescription,                             
                             FilesCount = m.Count(),
                           
                             UnknownsCount = m.Count(f1 => f1.CloudVerdict == CloudVerdict.Unknown),
                             MaliciousCount = m.Count(f1=>f1.CloudVerdict == CloudVerdict.Malware),
                             TotalDevicesScanned = i.AptScanResults.Where(r2=>r2.Succeeded).Count()

                         };

            return query2.ExecuteDbQuery();
        }
    }
}
