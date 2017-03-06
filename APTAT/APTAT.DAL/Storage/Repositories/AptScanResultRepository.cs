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
    public interface IAptScanResultRepository : IRepository<AptScanResult>
    {
        AptScanResult GetByTargetUidAndOperationUid(Guid targetUid, Guid operationUid);
        AptScanResultDTO[] GetByOperationUid(Guid operationUid);

    }

    internal sealed class AptScanResultRepository
        : Repository<AptEntities, AptScanResult, AptScanResult>, IAptScanResultRepository
    {
        public AptScanResultRepository(AptEntities dbContext)
            : base(dbContext)
        { }

        public AptScanResult GetByTargetUidAndOperationUid(Guid targetUid, Guid operationUid)
        {
            return DbSet.Include(c => c.OperationResult).Where(c => c.TargetUid == targetUid &&
                                                             c.OperationResult.OperationUid == operationUid)
                .ExecuteDbQuerySingleOrDefault();
        }

        public AptScanResultDTO[] GetByOperationUid(Guid operationUid)
        {
            var query = DbSet
                .Where(c => c.OperationResult.OperationUid == operationUid)
                .Include(c => c.Modules)
                .Select(c => new
                {
                    Models = c.Modules,
                    TotalCount = c.Modules.Count(),
                    UnknownsCount = c.Modules.Count(m => m.CloudVerdict == CloudVerdict.Unknown),
                    Data = c
                }).ToArray();
            
            var result = query.Select(r =>
               new AptScanResultDTO()
               {
                   ScanResult = r.Data,
                   TotalCount = r.TotalCount,
                   UnknownsCount = r.UnknownsCount
               }).ToArray();
            
            return result;

        }

        protected override DbSet<AptScanResult> DbSet
        {
            get { return DbContext.AptScanResults; }
        }

        public override IQueryable<AptScanResult> EntityQuery
        {
            get { return DbContext.AptScanResults; }
        }
    }
}
