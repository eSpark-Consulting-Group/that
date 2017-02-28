using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace APTAT.DAL.Storage.Core.Sorting
{
    internal interface ISortingKey<TSource>
    {
        IOrderedQueryable<TSource> OrderBy(IQueryable<TSource> source);
        IOrderedQueryable<TSource> ThenBy(IOrderedQueryable<TSource> source);
    }

    internal sealed class SortingKeyInternal<TSource> : ISortingKey<TSource>
    {
        private readonly ISortingExpression<TSource> _sortingExpression;
        private readonly ListSortDirection _sortOrder;

        private SortingKeyInternal(ISortingExpression<TSource> sortingExpression, ListSortDirection sortOrder)
        {
            if (sortingExpression == null)
                throw new ArgumentNullException("sortingExpression");
            _sortingExpression = sortingExpression;
            _sortOrder = sortOrder;
        }

        public ListSortDirection SortOrder
        {
            get { return _sortOrder; }
        }

        public IOrderedQueryable<TSource> OrderBy(IQueryable<TSource> source)
        {
            return SortingExpression.OrderBy(source, SortOrder);
        }

        public IOrderedQueryable<TSource> ThenBy(IOrderedQueryable<TSource> source)
        {
            return SortingExpression.ThenBy(source, SortOrder);
        }

        internal ISortingExpression<TSource> SortingExpression
        {
            get { return _sortingExpression; }
        }

        public static SortingKeyInternal<TSource> Create<TKey>(Expression<Func<TSource, TKey>> propertySelector, ListSortDirection sortOrder)
        {
            return new SortingKeyInternal<TSource>(new SortingExpression<TSource, TKey>(propertySelector), sortOrder);
        }

        public static SortingKeyInternal<TSource> Create(string keyName, ListSortDirection sortOrder)
        {
            ISortingExpression<TSource> sortingExpression = SortingExpressionFactory<TSource>.GetSortingExpression(keyName);
            return new SortingKeyInternal<TSource>(sortingExpression, sortOrder);
        }

        public static SortingKeyInternal<TSource> Create(SortingKey sortingKey)
        {
            if (sortingKey == null)
                throw new ArgumentNullException("sortingKey");
            return Create(sortingKey.KeyName, sortingKey.SortOrder);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", SortingExpression.ToString(), SortOrder.ToString());
        }
    }

    //internal sealed class SortingKeyInternal<TEntity, TKey> : SortingKeyBase<TEntity>
    //{
    //    public SortingKeyInternal(Expression<Func<TEntity, TKey>> propertySelector, SortOrder sortOrder = SortOrder.Ascending)
    //        : base(new SortingExpression<TEntity, TKey>(propertySelector), sortOrder)
    //    {}

    //    public static SortingKeyInternal<TEntity, TKey>  Create(SortingKey<TEntity, TKey> sortingKey)
    //    {
    //        if (sortingKey == null) 
    //            throw new ArgumentNullException("sortingKey");
    //        return new SortingKeyInternal<TEntity, TKey>(sortingKey.PropertySelector, sortingKey.SortOrder);
    //    }
    //}

    //internal sealed class SortingKeyNameInternal<TEntity> : SortingKeyBase<TEntity>
    //{
    //    public SortingKeyNameInternal(string keyName, SortOrder sortOrder = SortOrder.Ascending)
    //        : base(SortingExpressionFactory<TEntity>.GetSortingExpression(keyName), sortOrder)
    //    {}

    //    public static SortingKeyNameInternal<TEntity> Create(SortingKeyName sortingKey)
    //    {
    //        if (sortingKey == null)
    //            throw new ArgumentNullException("sortingKey");
    //        return new SortingKeyNameInternal<TEntity>(sortingKey.KeyName, sortingKey.SortOrder);
    //    }
    //}
}
