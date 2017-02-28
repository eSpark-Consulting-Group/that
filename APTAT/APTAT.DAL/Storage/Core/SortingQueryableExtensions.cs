using System;
using System.Linq;
using APTAT.Core.Storage;
using APTAT.DAL.Storage.Core.Sorting;

namespace APTAT.DAL.Storage.Core
{
    public static class SortingQueryableExtensions
    {
        public static IOrderedQueryable<TSource> Sort<TSource>(this IQueryable<TSource> source, params SortingKey[] sortingKeys)
        {
            if (sortingKeys == null) throw new ArgumentNullException("sortingKeys");

            var sortingKeysInternal = Array.ConvertAll(sortingKeys, SortingKeyInternal<TSource>.Create);

            IOrderedQueryable<TSource> result = source.Sort(sortingKeysInternal);
            return result;
        }

        public static IOrderedQueryable<TSource> Sort<TSource>(this IQueryable<TSource> source, params string[] sortingKeys)
        {
            if (sortingKeys == null) throw new ArgumentNullException("sortingKeys");

            IOrderedQueryable<TSource> result = source.Sort(Array.ConvertAll(sortingKeys, sortingKey => new SortingKey(sortingKey)));
            return result;
        }

        public static IQueryable<TSource> SortAndPage<TSource>(this IQueryable<TSource> source, SortingAndPaging<TSource> sortingAndPaging)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (sortingAndPaging == null) throw new ArgumentNullException("sortingAndPaging");

            IQueryable<TSource> result = source;
            if (sortingAndPaging.HasSorting)
            {
                result = sortingAndPaging.Sorting.Sort(result);
            }
            if (sortingAndPaging.HasPaging)
            {
                result = PageInternal(result, sortingAndPaging.Paging);
            }

            return result;
        }

        public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, Paging paging)
        {
            if (source == null) throw new ArgumentNullException("source");
            return PageInternal(source, paging);
        }

        public static IOrderedQueryable<TSource> Sort<TSource>(this IQueryable<TSource> source, Sorting<TSource> sorting)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (sorting == null) throw new ArgumentNullException("sorting");
            IOrderedQueryable<TSource> result = sorting.Sort(source);

            return result;
        }


        internal static IOrderedQueryable<TSource> Sort<TSource>(this IQueryable<TSource> source, params ISortingKey<TSource>[] sortingKeys)
        {
            if (sortingKeys == null) throw new ArgumentNullException("sortingKeys");
            if (sortingKeys.Length == 0) throw new ArgumentOutOfRangeException("sortingKeys");

            IOrderedQueryable<TSource> result = sortingKeys[0].OrderBy(source);
            for (int i = 1; i < sortingKeys.Length; i++)
            {
                result = sortingKeys[i].ThenBy(result);
            }
            return result;
        }

        private static IQueryable<TSource> PageInternal<TSource>(IQueryable<TSource> source, Paging paging)
        {
            IQueryable<TSource> result = source;

            int skipCount = paging.Index * paging.Size;
            int takeCount = paging.Size;

            if (skipCount > 0) result = result.Skip(skipCount);
            if (takeCount > 0) result = result.Take(takeCount);

            return result;
        }
    }
}
