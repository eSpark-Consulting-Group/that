using System;
using System.Data.Entity;
using System.Linq;using APTAT.DAL;
using APTAT.DAL.Entities;
using APTAT.DAL.Storage.Core;

namespace APTAT.Core.Storage.Repositories
{
    public interface IValkyrieScansRepository : IRepository<ValkyrieScanInfo>
    {
        ValkyrieScanInfo GetByOperationUid(Guid operationUid);
    }

    internal sealed class ValkyrieScansRepository
        : Repository<AptEntities, ValkyrieScanInfo, ValkyrieScanInfo>, IValkyrieScansRepository
    {
        public ValkyrieScansRepository(AptEntities dbContext)
            : base(dbContext)
        { }

        protected override DbSet<ValkyrieScanInfo> DbSet
        {
            get { return DbContext.ValkyrieScans; }
        }

        public override IQueryable<ValkyrieScanInfo> EntityQuery
        {
            get { return DbContext.ValkyrieScans; }
        }

        public ValkyrieScanInfo GetByOperationUid(Guid operationUid)
        {
            return DbSet.Where(o => o.AptScanOperationResult.OperationUid == operationUid).ExecuteDbQueryFirstOrDefault();
        }
    }
}
