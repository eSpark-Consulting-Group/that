using System;
using System.Data.Entity;
using System.Linq;
using APTAT.DAL;
using APTAT.DAL.Entities;
using APTAT.DAL.Storage.Core;
using System.Diagnostics;

namespace APTAT.Core.Storage.Repositories
{
    public interface IAptScanOperationResultRepository : IRepository<AptScanOperationResult>
    {
        AptScanOperationResult GetByOperationUid(Guid operationUid);
        AptScanOperationResult[] Search(string keyworld);
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

        public AptScanOperationResult[] Search(string keyword)
        {
            DbContext.Database.Log = (x) => Debug.Write(x);
            var query = DbContext.AptScanOperaionResults;
            if (keyword != null)
            {
                query.Where(o => o.AptScanResults.Any(r => r.ComputerName.Contains(keyword) || r.Modules.Any(m => m.Name.Contains(keyword))));
            }
            return query.ExecuteDbQuery();
        }
    }
}
