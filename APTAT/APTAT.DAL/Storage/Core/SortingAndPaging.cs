using System.Text;
using APTAT.Core.Storage;

namespace APTAT.DAL.Storage.Core
{
    public static class SortingAndPaging
    {
        public static SortingAndPaging<TSource> Create<TSource>(Sorting<TSource> sorting = null, Paging paging = null)
        {
            return new SortingAndPaging<TSource>(sorting, paging);
        }

        public static SortingAndPaging<TSource> Create<TSource>(Paging paging)
        {
            return new SortingAndPaging<TSource>(paging);
        }
    }

    public sealed class SortingAndPaging<TSource>
    {
        private readonly Paging _paging;
        private readonly Sorting<TSource> _sorting;

        public SortingAndPaging(Sorting<TSource> sorting = null, Paging paging = null)
        {
            _paging = paging ?? new Paging();
            _sorting = sorting ?? new Sorting<TSource>();
        }

        public SortingAndPaging(Paging paging)
            : this(null, paging)
        {
        }

        public Paging Paging
        {
            get { return _paging; }
        }

        public Sorting<TSource> Sorting
        {
            get { return _sorting; }
        }

        public bool HasSorting
        {
            get { return Sorting.HasSorting; }
        }

        public bool HasPaging
        {
            get { return Paging.HasPaging; }
        }

        public override string ToString()
        {
            if (!HasSorting && !HasPaging)
            {
                return base.ToString();
            }

            StringBuilder sb;
            sb = new StringBuilder();
            if (HasSorting)
            {
                sb.Append(string.Format("Sorting={{{0}}}", Sorting.ToString()));
            }
            if (HasPaging)
            {
                if (HasSorting)
                    sb.Append(", ");
                sb.Append(string.Format("Paging={{{0}}}", Paging.ToString()));
            }
            return sb.ToString();
        }

        public static implicit operator SortingAndPaging<TSource>(Sorting<TSource> sorting)
        {
            return new SortingAndPaging<TSource>(sorting);
        }
    }
}
