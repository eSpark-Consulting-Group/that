using System.Linq;

namespace APTAT.DAL.Storage.Core
{
    public interface IQueryableAdapter<out TEntity> where TEntity : IEntity
    {
        IQueryable<TEntity> EntityQuery { get; }
    }
}
