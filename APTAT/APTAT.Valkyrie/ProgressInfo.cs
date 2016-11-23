namespace APTAT.Valkyrie
{
    public class ProgressInfo<T>
    {       
        private readonly int _totalCount;
        private readonly int _processedCount;

        private readonly T[] _results;
        public T[] Results
        {
            get
            {
                return _results;
            }
        }
        public static ProgressInfo<T> Empty
        {
            get { return  new ProgressInfo<T>(0,0, new T[]{});}
        }

        public ProgressInfo(int total, int processed, T[] results)
        {
            _results = results;
            _totalCount = total;
            _processedCount = processed;
        }

        public int TotalFileCount
        {
            get
            {
                return _totalCount;
            }
        }
        public int ProcessedFileCount
        {
            get
            {

                return _processedCount;
            }
        }

        public double Percentage 
        {
            get
            {
                if (TotalFileCount == 0) return 0;

                return ((double)ProcessedFileCount / (double)TotalFileCount) * 100d;
            }
        }

        public static ProgressInfo<T> CreateFromTotal(int totalFileCount)
        {
            return new ProgressInfo<T>(totalFileCount, 0, new T[] {});
        }
    }   
}
