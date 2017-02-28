using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Core.Storage;

namespace APTAT.DAL.Storage.Core
{
    /// <summary>
    /// A IStorageContextBase instance represents a combination of the Unit Of Work and Repository patterns such that
    /// it can be used to query from a database and group together changes that will then be written back to the store as a unit.
    /// </summary>
    public interface IStorageContextBase : IDisposable
    {
        /// <summary>
        /// Gets and sets the timeout value used for queries with this context. 
        /// A null value indicates that the default value of the underlying provider will be used. 
        /// </summary> 
        int? CommandTimeout { get; set; }

        /// <summary>
        /// Saves all changes made in this context to the underlying database. 
        /// </summary>
        /// <returns>
        /// The number of objects written to the underlying database.
        /// </returns>
        int SaveChanges();

        /// <summary>
        /// Saves all changes made in this context to the underlying database. 
        /// Makes several tryes to resolve optimistic concurrency exceptions with specified strategy.
        /// </summary>
        /// <param name="strategy">Specifies how to resolve optimistic concurrency conflicts.</param>
        /// <returns>
        /// The number of objects written to the underlying database.
        /// </returns>
        int SaveChanges(ConcurrencyResolutionStrategy strategy);

        IRepository<TEntity> GetRepository<TEntity>() where TEntity : IEntity;
    }

    public enum ConcurrencyResolutionStrategy
    {
        ThrowException = 0,
        ClientWins = 1,
        StoreWins = 2,
    }
}
