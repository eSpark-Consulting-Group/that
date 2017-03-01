using System;
using System.Data.Entity;
using System.Linq;
using APTAT.DAL;
using APTAT.DAL.Entities;
using APTAT.DAL.Storage.Core;

namespace APTAT.Core.Storage.Repositories
{
    public interface IAptScanResultRepository : IRepository<AptScanResult>
    {
        AptScanResult GetByTargetUidAndOperationUid(Guid targetUid, Guid operationUid);
        AptScanResult[] GetByOperationUid(Guid operationUid);

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

        public AptScanResult[] GetByOperationUid(Guid operationUid)
        {
            AptScanResult[] result = DbSet
                .Include(c => c.Modules)
                .Where(c => c.OperationResult.OperationUid == operationUid)
                .ToArray();
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
