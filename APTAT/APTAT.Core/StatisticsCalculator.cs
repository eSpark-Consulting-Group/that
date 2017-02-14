using System.Collections.Generic;
using APTAT.Core.Descent.Apt;
using System.Linq;
using APTAT.Core.Reports;

namespace APTAT.Core
{
    public static class StatisticsCalculator
    {
        public static IFilesStatistics CalculateFilesStatistics(IEnumerable<IAptScanResult> results)
        {
            int totalFilesCount = 0;
            int safeFilesCount = 0;
            int unknownFilesCount = 0;
            int malwareFilesCount = 0;
            int emptyFilesCount = 0;
            int skippedFilesCount = 0;
            int errorFilesCount = 0;
            int serverErrorFilesCount = 0;
            int pendingFilesCount = 0;
            foreach (var scanResult in results)
            {
                if (scanResult.ResultType == ScanResultType.Completed)
                {
                    totalFilesCount += scanResult.TotalFilesCount;
                    safeFilesCount += scanResult.SafeFilesCount;
                    unknownFilesCount += scanResult.UnknownFilesCount;
                    malwareFilesCount += scanResult.MalwareFilesCount;
                    emptyFilesCount += scanResult.EmptyFilesCount;
                    skippedFilesCount += scanResult.SkippedFilesCount;
                    errorFilesCount += scanResult.FailedToAnalyzeFilesCount;
                    serverErrorFilesCount += scanResult.NoVerdictFilesCount;
                    pendingFilesCount += scanResult.PendingAnalysisFilesCount;
                }
            }
            return new FilesStatistics
            {
                EmptyFilesCount = emptyFilesCount,
                MalwareFilesCount = malwareFilesCount,
                SafeFilesCount = safeFilesCount,
                TotalFilesCount = totalFilesCount,
                UnknownFilesCount = unknownFilesCount,
                SkippedFilesCount = skippedFilesCount,
                FailedToAnalyzeFilesCount = errorFilesCount,
                NoVerdictFilesCount = serverErrorFilesCount,
                PendingAnalysisFilesCount = pendingFilesCount
            };
        }

        public static void CalculateOperationStatistics(IEnumerable<IAptScanResult> results, out int failed,
            out int notPerformed, out int succeeded, out int total, out int canceled)
        {
            notPerformed = 0;
            succeeded = 0;
            failed = 0;
            canceled = 0;

            var scanResults = results as AptScanResult[] ?? results.ToArray();
            total = scanResults.Count();
            var groupped = from result in scanResults group result by result.ResultType into temp select temp;
            foreach (var item in groupped)
            {
                if (item.Key == ScanResultType.NotPerformed)
                    notPerformed = item.Count();
                else if (item.Key == ScanResultType.Failed)
                    failed = item.Count();
                else if (item.Key == ScanResultType.Completed)
                    succeeded = item.Count();
                else if (item.Key == ScanResultType.Canceled)
                    canceled = item.Count();
            }
        }
    }
}
