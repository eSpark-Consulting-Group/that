namespace APTAT.Core
{
    public class FilesStatistics
    {
        public int TotalFilesCound { get; set; }
        public int SafeFilesCount { get; set; }
        public int UnknownFilesCount { get; set; }
        public int MalwareFilesCount { get; set; }
        public int EmptyFilesCount { get; set; }
        public int FailedToAnalyzeFilesCount { get; set; }
        public int SkippedFilesCount { get; set; }
        public int NoVerdictFilesCount { get; set; }
    }
}