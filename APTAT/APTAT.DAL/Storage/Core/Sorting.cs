using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using APTAT.Core.Storage;
using APTAT.DAL.Storage.Core.Sorting;

namespace APTAT.DAL.Storage.Core
{
    public sealed class Sorting<TSource>
    {
        private readonly List<ISortingKey<TSource>> _sortingKeys;

        internal Sorting()
        {
            _sortingKeys = new List<ISortingKey<TSource>>(10);
        }

        public Sorting<TSource> ThenBy(string key, ListSortDirection sortOrder)
        {
            Add(SortingKeyInternal<TSource>.Create(key, sortOrder));
            return this;
        }

        public Sorting<TSource> ThenBy(params SortingKey[] sortingKeys)
        {
            if (sortingKeys == null) throw new ArgumentNullException("sortingKeys");
            foreach (var sortingKey in sortingKeys)
            {
                Add(SortingKeyInternal<TSource>.Create(sortingKey));
            }

            return this;
        }

        public Sorting<TSource> ThenBy<TKey>(Expression<Func<TSource, TKey>> propertySelector, ListSortDirection sortOrder = ListSortDirection.Ascending)
        {
            Add(SortingKeyInternal<TSource>.Create(propertySelector, sortOrder));
            return this;
        }

        public static Sorting<TSource> OrderBy(string key, ListSortDirection sortOrder)
        {
            var instance = new Sorting<TSource>();
            return instance.ThenBy(key, sortOrder);
        }

        public static Sorting<TSource> OrderBy(params SortingKey[] sortingKeys)
        {
            var instance = new Sorting<TSource>();
            return instance.ThenBy(sortingKeys);
        }

        public static Sorting<TSource> OrderBy<TKey>(Expression<Func<TSource, TKey>> propertySelector, ListSortDirection sortOrder = ListSortDirection.Ascending)
        {
            var instance = new Sorting<TSource>();
            return instance.ThenBy(propertySelector, sortOrder);
        }

        private void Add(ISortingKey<TSource> sortingKey)
        {
            _sortingKeys.Add(sortingKey);
        }

        public bool HasSorting
        {
            get { return _sortingKeys.Count > 0; }
        }

        internal IOrderedQueryable<TSource> Sort(IQueryable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (!HasSorting)
                throw new InvalidOperationException("No sorting keys");

            IOrderedQueryable<TSource> result = source.Sort(_sortingKeys.ToArray());
            return result;
        }

        public override string ToString()
        {
            if (!HasSorting)
            {
                return base.ToString();
            }


            var sb = new StringBuilder(_sortingKeys.Count * 128);

            sb.Append(_sortingKeys[0].ToString());
            for (int index = 1; index < _sortingKeys.Count; index++)
            {
                var sortingKey = _sortingKeys[index];
                sb.Append(", ");
                sb.Append(sortingKey.ToString());
            }

            return sb.ToString();
        }
    }
}
