using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using APTAT.Core.Storage;

namespace APTAT.DAL.Storage.Core
{
    internal abstract class StorageContextBase<TDbContext> : IStorageContextBase
            where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;

        private bool _disposed;


        private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>(100);


        protected StorageContextBase(TDbContext dbContext)
        {
            if (dbContext == null) throw new ArgumentNullException("dbContext");
            _dbContext = dbContext;

            _disposed = false;
        }

        public TDbContext DbContext
        {
            get { return _dbContext; }
        }

        public virtual int? CommandTimeout
        {
            get
            {
                return ((IObjectContextAdapter)DbContext).ObjectContext.CommandTimeout;
            }
            set
            {
                ((IObjectContextAdapter)DbContext).ObjectContext.CommandTimeout = value;
            }
        }

        public int SaveChanges()
        {
            return DbContext.SaveChangesWithErrorsHandling(ConcurrencyResolutionStrategy.ThrowException);
        }

        public int SaveChanges(ConcurrencyResolutionStrategy strategy)
        {
            return DbContext.SaveChangesWithErrorsHandling(strategy);
        }


        public void Dispose()
        {
            if (!_disposed)
            {

                if (_dbContext != null)
                {
                    _dbContext.Dispose();
                }
                _disposed = true;
            }
        }


        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : IEntity
        {
            return (IRepository<TEntity>)_repositories[typeof(TEntity)];
        }

        protected TRepository RegisterRepository<TRepository>(TRepository repository)
            where TRepository : IRepositoryBase
        {
            _repositories.Add(repository.EntityType, repository);
            return repository;
        }
    }
}
