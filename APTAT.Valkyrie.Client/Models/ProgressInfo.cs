namespace APTAT.Valkyrie.Client.Models
{
    public class ProgressInfo
    {
        private readonly int _totalCount;
        private readonly int _processedCount;
        private readonly int _malwareFilesCount;

        public static ProgressInfo Empty
        {
            get { return  new ProgressInfo(0,0,0);}
        }

        public ProgressInfo(int total, int processed, int malware)
        {
            _totalCount = total;
            _processedCount = processed;
            _malwareFilesCount = malware;
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
    }
}
