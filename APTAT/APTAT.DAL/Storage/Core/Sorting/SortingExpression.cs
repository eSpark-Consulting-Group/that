using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace APTAT.DAL.Storage.Core.Sorting
{
    internal interface ISortingExpression<TSource>
    {
        IOrderedQueryable<TSource> OrderBy(IQueryable<TSource> source, ListSortDirection sortOrder);
        IOrderedQueryable<TSource> ThenBy(IOrderedQueryable<TSource> source, ListSortDirection sortOrder);
    }

    internal sealed class SortingExpression<TSource, TKey> : ISortingExpression<TSource>
    {
        private readonly Expression<Func<TSource, TKey>> _propertySelector;

        public SortingExpression(Expression<Func<TSource, TKey>> propertySelector)
        {
            if (propertySelector == null)
                throw new ArgumentNullException("propertySelector");
            _propertySelector = propertySelector;
        }

        public Expression<Func<TSource, TKey>> PropertySelector
        {
            get { return _propertySelector; }
        }


        public IOrderedQueryable<TSource> OrderBy(IQueryable<TSource> source, ListSortDirection sortOrder)
        {
            IOrderedQueryable<TSource> result = sortOrder == ListSortDirection.Ascending
                                                    ? source.OrderBy(PropertySelector)
                                                    : source.OrderByDescending(PropertySelector);
            return result;
        }

        public IOrderedQueryable<TSource> ThenBy(IOrderedQueryable<TSource> source, ListSortDirection sortOrder)
        {
            IOrderedQueryable<TSource> result = sortOrder == ListSortDirection.Ascending
                                                    ? source.ThenBy(PropertySelector)
                                                    : source.ThenByDescending(PropertySelector);
            return result;
        }

        public override string ToString()
        {
            return PropertySelector.ToString();
        }
    }
}
