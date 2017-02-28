using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace APTAT.DAL.Storage.Core
{
    internal static class ExecuteDbQueryExtensions
    {
        public static TSource[] ExecuteDbQuery<TSource>(this IQueryable<TSource> query,
            SortingAndPaging<TSource> sortingAndPaging = null, bool cacheEntitiesInContext = true)
            where TSource : class
        {
            if (query == null) throw new ArgumentNullException("query");

            QueryWithPaging<TSource> resultQuery = BuildQuery(query, sortingAndPaging, cacheEntitiesInContext);

            ExecuteCalcTotalPages(sortingAndPaging, resultQuery.TotalItemsCountQuery);
            var result = ExecuteQueryInternal(resultQuery.ResultQuery);

            return result;
        }

        public static TSource[] ExecuteDbQuery<TSource>(this IQueryable<TSource> query)
        {
            return ExecuteQueryInternal(query);
        }

        public static TSource[] ExecuteDbQuery<TSource>(this DbSqlQuery<TSource> query, bool cacheEntitiesInContext = true) where TSource : class
        {
            var resultQuery = cacheEntitiesInContext ? query : query.AsNoTracking();
            return ExecuteQueryInternal(resultQuery);
        }

        public static TSource[] ExecuteDbQuery<TSource>(this DbSqlQuery<TSource> query) where TSource : class
        {
            return ExecuteQueryInternal(query);
        }

        public static bool ProcessDbQuery<TSource>(this IQueryable<TSource> query, Func<TSource, bool> entityProcessAction,
           SortingAndPaging<TSource> sortingAndPaging = null, bool cacheEntitiesInContext = true) where TSource : class

        {
            if (query == null) throw new ArgumentNullException("query");
            if (entityProcessAction == null) throw new ArgumentNullException("entityProcessAction");

            QueryWithPaging<TSource> resultQuery = BuildQuery(query, sortingAndPaging, cacheEntitiesInContext);

            ExecuteCalcTotalPages(sortingAndPaging, resultQuery.TotalItemsCountQuery);
            bool allItemsProcessed = ProcessQueryInternal(resultQuery.ResultQuery, entityProcessAction);

            return allItemsProcessed;
        }

        public static bool ProcessDbQuery<TSource>(this IQueryable<TSource> query, Func<TSource, bool> entityProcessAction)
        {
            return ProcessQueryInternal(query, entityProcessAction);
        }

        public static bool ProcessDbQuery<TSource>(this DbSqlQuery<TSource> query, Func<TSource, bool> entityProcessAction) where TSource : class
        {
            return ProcessQueryInternal(query, entityProcessAction);
        }


        public static TSource ExecuteDbQueryFirstOrDefault<TSource>(this IQueryable<TSource> query,
            Sorting<TSource> sorting = null, bool cacheEntitiesInContext = true)
            where TSource : class
        {
            if (query == null) throw new ArgumentNullException("query");

            QueryWithPaging<TSource> resultQuery = BuildQuery(query, sorting, cacheEntitiesInContext);

            return ExecuteFirstOrDefaultQueryInternal(resultQuery.ResultQuery);
        }

        public static TSource ExecuteDbQuerySingleOrDefault<TSource>(this IQueryable<TSource> query, bool cacheEntitiesInContext = true)
            where TSource : class
        {
            if (query == null) throw new ArgumentNullException("query");

            QueryWithPaging<TSource> resultQuery = BuildQuery(query, null, cacheEntitiesInContext);

            return ExecuteSingleOrDefaultQueryInternal(resultQuery.ResultQuery);
        }

        public static int ExecuteDbQueryCount<TSource>(this IQueryable<TSource> query)
        {
            if (query == null) throw new ArgumentNullException("query");

            return ExecuteCountQueryInternal(query);
        }

        public static TSource ExecuteDbQueryMax<TSource>(this IQueryable<TSource> query)
        {
            if (query == null) throw new ArgumentNullException("query");

            return ExecuteMaxQueryInternal(query);
        }

        public static bool ExecuteDbQueryAny<TSource>(this IQueryable<TSource> query)
        {
            if (query == null) throw new ArgumentNullException("query");

            return ExecuteAnyQueryInternal(query);
        }

        private struct QueryWithPaging<TSource>
        {
            public QueryWithPaging(IQueryable<TSource> resultQuery, IQueryable<TSource> totalCountQuery)
                : this()
            {
                ResultQuery = resultQuery;
                TotalItemsCountQuery = totalCountQuery;
            }

            public IQueryable<TSource> ResultQuery { get; private set; }
            public IQueryable<TSource> TotalItemsCountQuery { get; private set; }
        }

        private static QueryWithPaging<TSource> BuildQuery<TSource>(IQueryable<TSource> query,
            SortingAndPaging<TSource> sortingAndPaging, bool cacheEntitiesInContext)
           where TSource : class
        {
            IQueryable<TSource> resultQuery = query;
            IQueryable<TSource> totalCountQuery = query;
            if (sortingAndPaging != null)
            {
                resultQuery = resultQuery.SortAndPage(sortingAndPaging);
            }

            resultQuery = resultQuery.AsCachedInContext(cacheEntitiesInContext);
            return new QueryWithPaging<TSource>(resultQuery, totalCountQuery);
        }


        private static IQueryable<TSource> AsCachedInContext<TSource>(this IQueryable<TSource> source, bool cacheEntities) where TSource : class
        {
            return cacheEntities ? source : source.AsNoTracking();
        }

        private static void ExecuteCalcTotalPages<TEntity>(SortingAndPaging<TEntity> sortingAndPaging, IQueryable<TEntity> totalItemsCountQuery)
        {
            if (sortingAndPaging != null && sortingAndPaging.HasPaging && sortingAndPaging.Paging.CalculateTotalPages)
            {
                int totalItemsCount = ExecuteCountQueryInternal(totalItemsCountQuery);
                sortingAndPaging.Paging.SetTotalPages(totalItemsCount);
            }
        }

        #region methods that makes the real query in the database
        private static bool ProcessQueryInternal<TSource>(IEnumerable<TSource> query, Func<TSource, bool> entityProcessAction)
        {
            bool allItemsProcessed = query.All(entityProcessAction);
            return allItemsProcessed;
        }

        private static TSource[] ExecuteQueryInternal<TSource>(IEnumerable<TSource> resultQuery)
        {
            TSource[] result = resultQuery.ToArray();
            return result;
        }

        private static TSource ExecuteFirstOrDefaultQueryInternal<TSource>(IQueryable<TSource> resultQuery)
        {
            TSource result = resultQuery.FirstOrDefault();
            return result;
        }

        private static TSource ExecuteSingleOrDefaultQueryInternal<TSource>(IQueryable<TSource> resultQuery)
        {
            TSource result = resultQuery.SingleOrDefault();
            return result;
        }

        private static int ExecuteCountQueryInternal<TSource>(IQueryable<TSource> resultQuery)
        {
            int result = resultQuery.Count();
            return result;
        }

        private static TSource ExecuteMaxQueryInternal<TSource>(IQueryable<TSource> resultQuery)
        {
            TSource result = resultQuery.Max();
            return result;
        }

        private static bool ExecuteAnyQueryInternal<TSource>(IQueryable<TSource> resultQuery)
        {
            bool result = resultQuery.Any();
            return result;
        }

        public static void ExecuteDbContextActionWithoutAutoDetectChanges(this DbContext dbContext, Action dbContextAction)
        {
            if (dbContext == null) throw new ArgumentNullException("dbContext");
            if (dbContextAction == null) throw new ArgumentNullException("dbContextAction");

            bool autoDetectChangesEnabled = dbContext.Configuration.AutoDetectChangesEnabled;
            try
            {
                dbContext.Configuration.AutoDetectChangesEnabled = false;

                dbContextAction();
            }
            finally
            {
                dbContext.Configuration.AutoDetectChangesEnabled = autoDetectChangesEnabled;
            }
        }

        #endregion
    }
}
