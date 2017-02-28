using System.Data.Entity;
using System.Linq;using APTAT.DAL;
using APTAT.DAL.Entities;
using APTAT.DAL.Storage.Core;

namespace APTAT.Core.Storage.Repositories
{
    public interface IModulesRepository : IRepository<Module>
    {
    }

    internal sealed class ModulesRepository
        : Repository<AptEntities, Module, Module>, IModulesRepository
    {
        public ModulesRepository(AptEntities dbContext)
            : base(dbContext)
        { }

        protected override DbSet<Module> DbSet
        {
            get { return DbContext.Modules; }
        }

        public override IQueryable<Module> EntityQuery
        {
            get { return DbContext.Modules; }
        }
    }
}
