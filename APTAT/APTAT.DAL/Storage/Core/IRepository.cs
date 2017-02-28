using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace APTAT.DAL.Storage.Core
{
    public interface IRepository<TEntity> : IRepositoryBase
        where TEntity : IEntity
    {
        TEntity ExecuteFind(params object[] keyValues);
        TEntity[] GetAll(SortingAndPaging<TEntity> sortingAndPaging, bool cacheEntitiesInContext = true);
        int GetCount();
        TEntity GetFirstOrDefault(Sorting<TEntity> sorting, bool cacheEntitiesInContext = true);
        TEntity GetSingleOrDefault(bool cacheEntitiesInContext = true);

        TEntity Add(TEntity entity);

        TEntity Remove(TEntity entity);

        TEntity AddOrUpdate(TEntity entity);

        /// <summary>
        /// don't use it for removing many records. Use AddMany() or write special method in IStorage
        /// </summary>
        /// <param name="entity"></param>
        TEntity ForcedAddOrUpdate(TEntity entity);

        TEntity ForcedUpdate<TProperty>(TEntity existingEntity, Expression<Func<TEntity, TProperty>> propertyToUpdate);
        TEntity ForcedUpdate(TEntity existingEntity, ICollection<string> propertiesToUpdate);

        /// <summary>
        /// don't use it for removing many records or write special method in IStorage.
        /// </summary>
        /// <param name="existingEntity"></param>
        void ForcedRemove(TEntity existingEntity);

        void AddMany(ICollection<TEntity> entities);
    }

    public interface IRepositoryBase
    {
        Type EntityType { get; }
    }
}
