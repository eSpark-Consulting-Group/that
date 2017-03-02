using System.Data.Entity;
using System.Linq;
using APTAT.DAL;
using APTAT.DAL.Entities;
using APTAT.DAL.Storage.Core;

namespace APTAT.Core.Storage.Repositories
{
    public interface IValkyrieFilesRepository : IRepository<ValkyrieFileInfo>
    {
        ValkyrieFileInfo[] GetByScanId(int scanId);
        IQueryable<ValkyrieFileInfo> EntityQuery { get; }
    }

    internal sealed class ValkyrieFilesRepository
        : Repository<AptEntities, ValkyrieFileInfo, ValkyrieFileInfo>, IValkyrieFilesRepository
    {
        public ValkyrieFilesRepository(AptEntities dbContext)
            : base(dbContext)
        { }

        protected override DbSet<ValkyrieFileInfo> DbSet
        {
            get { return DbContext.ValkyrieFiles; }
        }

        public override IQueryable<ValkyrieFileInfo> EntityQuery
        {
            get { return DbContext.ValkyrieFiles; }
        }

        public ValkyrieFileInfo[] GetByScanId(int scanId)
        {
            return DbSet.Where(s => s.ValkyrieScanInfoId == scanId).ExecuteDbQuery();
        }
    }
}
