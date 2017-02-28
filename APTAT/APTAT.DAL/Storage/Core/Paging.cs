namespace APTAT.DAL.Storage.Core
{
    public sealed class Paging
    {
        private readonly bool _calculateTotalPages;

        /// <summary>
        /// Initializes a new instance of the <see cref="Paging"/> class
        /// </summary>
        /// <param name="pageSize">Number of items on the page </param>
        /// <param name="pageIndex">Zero-based index of the page</param>
        /// <param name="calculateTotalPages">Flag specifying wether an additional query should be performed to calculate the total pages number</param>
        public Paging(int pageSize, int pageIndex, bool calculateTotalPages = false)
        {
            Size = pageSize;
            Index = pageIndex;
            _calculateTotalPages = calculateTotalPages;
        }

        public Paging() : this(0, 0) { }

        /// <summary>
        /// Flag specifying whether an additional query should be performed to calculate the total number of pages.
        /// </summary>
        public bool CalculateTotalPages
        {
            get { return _calculateTotalPages; }
        }

        /// <summary>
        /// Zero-based index of a page
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Number of items in a page 
        /// </summary>
        public int Size { get; set; }

        public bool HasPaging
        {
            get { return Size > 0 && Index >= 0; }
        }

        public int TotalPages { get; private set; }

        public override string ToString()
        {
            if (!HasPaging)
            {
                return base.ToString();
            }

            return string.Format("Index={0}, Size={1}", Index, Size);
        }

        internal void SetTotalPages(int totalItemsCount)
        {
            int result = 0;

            if (Size > 0 && totalItemsCount > 0)
            {
                int fullPages = totalItemsCount / Size;
                bool hasPartiallyFilledPage = (totalItemsCount % Size) > 0;

                result = hasPartiallyFilledPage
                    ? fullPages + 1
                    : fullPages;
            }

            TotalPages = result;
        }
    }
}
