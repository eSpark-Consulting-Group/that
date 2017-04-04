using System;

namespace APTAT.Core.Reports
{
    public static class ScanResultMapper
    {
        public static ScanResultType ToBusiness(this Core.ScanResultType scanResultType)
        {
            ScanResultType resultType = ScanResultType.NotPerformed;
            switch (scanResultType)
            {
                case Core.ScanResultType.NotPerformed:
                    resultType = ScanResultType.NotPerformed;
                    break;
                case Core.ScanResultType.InProgress:
                    throw new NotSupportedException(nameof(Core.ScanResultType.InProgress));
                case Core.ScanResultType.Completed:
                    resultType = ScanResultType.Completed;
                    break;
                case Core.ScanResultType.Failed:
                    resultType = ScanResultType.Failed;
                    break;
                case Core.ScanResultType.Canceled:
                    resultType = ScanResultType.Canceled;
                    break;
                default:
                    break;
            }

            return resultType;
        }
    }
}
