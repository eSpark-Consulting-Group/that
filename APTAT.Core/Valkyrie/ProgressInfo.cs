namespace APTAT.Core.Valkyrie
{
    public class ProgressInfo<T>
    {
       
        private readonly int _totalCount;
        private readonly int _processedCount;
        private readonly int _malwareFilesCount;
        private readonly int _unknownFileCount;

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
            get { return  new ProgressInfo<T>(0,0,0,0, new T[]{});}
        }

        public ProgressInfo(int total, int processed, int malware, int unknownFileCount, T[] results)
        {
            _results = results;
            _totalCount = total;
            _processedCount = processed;
            _malwareFilesCount = malware;
            _unknownFileCount = unknownFileCount;
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

        public int MalwareFilesCount
        {
            get
            {
                return _malwareFilesCount;
            }
        }

        public int UnknownFileCount
        {
            get
            {
                return _unknownFileCount;
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
            return new ProgressInfo<T>(totalFileCount, 0, 0,0, new T[] {});
        }
    }

   
}
