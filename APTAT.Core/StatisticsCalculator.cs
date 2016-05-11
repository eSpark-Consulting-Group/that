using System.Collections.Generic;

namespace APTAT.Core
{
    public static class StatisticsCalculator
    {
        public static FilesStatistics Calculate(IEnumerable<ScanResult> results)
        {
            int totalFilesCound = 0;
            int safeFilesCount = 0;
            int unknownFilesCount = 0;
            int malwareFilesCount = 0;
            int emptyFilesCount = 0;
            int skippedFilesCount = 0;

            foreach (var scanResult in results)
            {
                if (scanResult.ResultType == ScanResultType.Completed)
                {
                    totalFilesCound += scanResult.TotalFilesCount;
                    safeFilesCount += scanResult.SafeFilesCount;
                    unknownFilesCount += scanResult.UnknownFilesCount;
                    malwareFilesCount += scanResult.MalwareFilesCount;
                    emptyFilesCount += scanResult.EmptyFilesCount;
                    skippedFilesCount += scanResult.SkippedFilesCount;
                    
                }
            }
            return new FilesStatistics {EmptyFilesCount = emptyFilesCount, MalwareFilesCount = malwareFilesCount, SafeFilesCount = safeFilesCount, TotalFilesCound = totalFilesCound, UnknownFilesCount = unknownFilesCount, SkippedFilesCount = skippedFilesCount};
        }
    }

    public class FilesStatistics
    {
        public int TotalFilesCound { get; set; }
        public int SafeFilesCount { get; set; }
        public int UnknownFilesCount { get; set; }
        public int MalwareFilesCount { get; set; }
        public int EmptyFilesCount { get; set; }
        public int SkippedFilesCount { get; set; }
    }
}
