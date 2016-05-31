using System.Collections.Generic;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core
{
    public static class StatisticsCalculator
    {
        public static FilesStatistics Calculate(IEnumerable<AptScanResult> results)
        {
            int totalFilesCound = 0;
            int safeFilesCount = 0;
            int unknownFilesCount = 0;
            int malwareFilesCount = 0;
            int emptyFilesCount = 0;
            int skippedFilesCount = 0;
            int errorFilesCount = 0;
            int serverErrorFilesCount = 0;

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
                    errorFilesCount += scanResult.FailedToAnalyzeFilesCount;
                    serverErrorFilesCount += scanResult.NoVerdictFilesCount;
                }
            }
            return new FilesStatistics
            {
                EmptyFilesCount = emptyFilesCount,
                MalwareFilesCount = malwareFilesCount,
                SafeFilesCount = safeFilesCount,
                TotalFilesCound = totalFilesCound,
                UnknownFilesCount = unknownFilesCount,
                SkippedFilesCount = skippedFilesCount,
                FailedToAnalyzeFilesCount = errorFilesCount,
                NoVerdictFilesCount = serverErrorFilesCount

            };
        }
    }
}
