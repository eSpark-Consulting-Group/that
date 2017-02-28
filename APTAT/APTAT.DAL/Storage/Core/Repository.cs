using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace APTAT.DAL.Storage.Core
{
    internal abstract class Repository<TDbContext, TEntity, TDbSetEntity> : IRepository<TEntity>, IQueryableAdapter<TEntity>
        where TEntity : class, IEntity, TDbSetEntity
        where TDbSetEntity : class, IEntity
        where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;


        protected Repository(TDbContext dbContext)
        {
            if (dbContext == null) throw new ArgumentNullException("dbContext");
            _dbContext = dbContext;
        }

        protected TDbContext DbContext
        {
            get { return _dbContext; }
        }

        protected abstract DbSet<TDbSetEntity> DbSet { get; }

        public abstract IQueryable<TEntity> EntityQuery { get; }


        public virtual TEntity[] GetAll(SortingAndPaging<TEntity> sortingAndPaging, bool cacheEntitiesInContext = true)
        {
            var result = EntityQuery.ExecuteDbQuery(sortingAndPaging, cacheEntitiesInContext);

            return result;
        }

        public virtual int GetCount()
        {
            var result = DbSet.ExecuteDbQueryCount();

            return result;
        }

        public virtual TEntity GetFirstOrDefault(Sorting<TEntity> sorting, bool cacheEntitiesInContext = true)
        {
            var result = EntityQuery.ExecuteDbQueryFirstOrDefault(sorting, cacheEntitiesInContext);

            return result;
        }

        public virtual TEntity GetSingleOrDefault(bool cacheEntitiesInContext = true)
        {
            var result = EntityQuery.ExecuteDbQuerySingleOrDefault(cacheEntitiesInContext);

            return result;
        }

        public virtual TEntity Add(TEntity entity)
        {
            return (TEntity)DbSet.Add(entity);
        }

        public virtual void AddMany(ICollection<TEntity> entities)
        {
            if (entities == null) throw new ArgumentNullException("entities");
            if (entities.Count > 0)
                DbContext.ExecuteDbContextActionWithoutAutoDetectChanges(() => Add(entities));
        }

        public virtual TEntity Remove(TEntity entity)
        {
            return (TEntity)DbSet.Remove(entity);
        }

        public virtual TEntity AddOrUpdate(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            if (entity.EntityKeyValuesAreEmpty)
            {
                DbSet.Add(entity);
            }
            else
            {
                var tracked = DbSet.Find(entity.EntityKeyValues);
                if (tracked == null)
                    throw new DbUpdateException("Entity is not found");
                DbContext.Entry(tracked).CurrentValues.SetValues(entity); // only those properties with values that are different will be marked as modified.
                return (TEntity)tracked;
            }


            return entity;
        }


        public virtual TEntity ForcedAddOrUpdate(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            DbContext.Entry(entity).State = entity.EntityKeyValuesAreEmpty
                                                        ? EntityState.Added
                                                        : EntityState.Modified;
            return entity;
        }

        public virtual TEntity ForcedUpdate<TProperty>(TEntity existingEntity, Expression<Func<TEntity, TProperty>> propertyToUpdate)
        {
            if (propertyToUpdate == null)
                throw new ArgumentNullException("propertyToUpdate");

            var entityEntry = PrepareForcedUpdate(existingEntity);

            entityEntry.Property(propertyToUpdate).IsModified = true;

            return existingEntity;
        }

        private DbEntityEntry<TEntity> PrepareForcedUpdate(TEntity existingEntity)
        {
            DbContext.Configuration.ValidateOnSaveEnabled = false;
            DbContext.Configuration.AutoDetectChangesEnabled = false;
            // if AutoDetectChangesEnabled equals true the method DbContext.Entry() works very slow when there are a lot of entities in DbContext

            DbEntityEntry<TEntity> entityEntry = DbContext.Entry(existingEntity);
            switch (entityEntry.State)
            {
                case EntityState.Added:
                case EntityState.Deleted:
                    throw new InvalidOperationException("Invalid entity state");
                case EntityState.Detached:
                    DbSet.Attach(existingEntity);
                    break;
            }
            return entityEntry;
        }

        public virtual TEntity ForcedUpdate(TEntity existingEntity, ICollection<string> propertiesToUpdate)
        {
            if (propertiesToUpdate == null)
                throw new ArgumentNullException("propertiesToUpdate");

            var entityEntry = PrepareForcedUpdate(existingEntity);

            foreach (var propertyName in propertiesToUpdate)
            {
                entityEntry.Property(propertyName).IsModified = true;
            }

            return existingEntity;
        }

        public virtual void ForcedRemove(TEntity existingEntity)
        {
            DbContext.Entry(existingEntity).State = EntityState.Deleted;
        }

        public virtual TEntity ExecuteFind(params object[] keyValues)
        {
            return (TEntity)DbSet.Find(keyValues);
        }

        public Type EntityType
        {
            get { return typeof(TEntity); }
        }

        /*

            public IQueryable<TEntity> GetQuery(IEnumerable<Expression<Func<TEntity, object>>> includeSelectors = null,
                                                Expression<Func<TEntity, bool>> wherePredicate = null, IEnumerable<SortingKey<TEntity>> sortingKeys = null,
                                                int takeCount = 0, int skipCount = 0)
            {
                return DbQuery.BuildQuery(includeSelectors, wherePredicate, sortingKeys, takeCount, skipCount);
            }

            public IQueryable<TResult> GetQuery<TResult>(Expression<Func<TEntity, TResult>> selector,
                                                         IEnumerable<Expression<Func<TEntity, object>>> includeSelectors = null, Expression<Func<TEntity, bool>> wherePredicate = null,
                                                         IEnumerable<SortingKey<TEntity>> sortingKeys = null, int takeCount = 0, int skipCount = 0) 
            {
                return DbQuery.BuildQuery(selector, includeSelectors, wherePredicate, sortingKeys, takeCount, skipCount);
            }

            public IEnumerable<TEntity> ExecuteQuery(IEnumerable<Expression<Func<TEntity, object>>> includeSelectors, 
                                                     Expression<Func<TEntity, bool>> wherePredicate, IEnumerable<SortingKey<TEntity>> sortingKeys = null, 
                                                     int takeCount = 0, int skipCount = 0)
            {
                IQueryable<TEntity> query = DbQuery.BuildQuery(includeSelectors, wherePredicate, sortingKeys, takeCount, skipCount);
                var results = query.ExecuteQuery();
                return results;
            }

            public IEnumerable<TResult> ExecuteQuery<TResult>(Expression<Func<TEntity, TResult>> selector,
                                                              IEnumerable<Expression<Func<TEntity, object>>> includeSelectors = null, Expression<Func<TEntity, bool>> wherePredicate = null,
                                                              IEnumerable<SortingKey<TEntity>> sortingKeys = null, int takeCount = 0, int skipCount = 0) where TResult : class
            {
                IQueryable<TResult> query = DbQuery.BuildQuery(selector, includeSelectors, wherePredicate, sortingKeys, takeCount, skipCount);
                var results = query.ExecuteQuery();
                return results;
            }




            public void ProcessQuery(Func<TEntity, bool> entityProcessAction, IEnumerable<Expression<Func<TEntity, object>>> includeSelectors,
                                     Expression<Func<TEntity, bool>> wherePredicate, IEnumerable<SortingKey<TEntity>> sortingKeys = null,
                                     int takeCount = 0, int skipCount = 0)
            {
                var query = DbQuery.BuildQuery(includeSelectors, wherePredicate, sortingKeys, takeCount, skipCount);
                query.ProcessQuery(entityProcessAction);
            }

            public void ProcessQuery<TResult>(Func<TResult, bool> entityProcessAction, Expression<Func<TEntity, TResult>> selector, IEnumerable<Expression<Func<TEntity, object>>> includeSelectors,
                                              Expression<Func<TEntity, bool>> wherePredicate, IEnumerable<SortingKey<TEntity>> sortingKeys = null,
                                              int takeCount = 0, int skipCount = 0)
            {
                var query = DbQuery.BuildQuery(selector, includeSelectors, wherePredicate, sortingKeys, takeCount, skipCount);
                query.ProcessQuery(entityProcessAction);
            }
         */

        private void Add(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                DbSet.Add(entity);
            }
        }

    }
}
