using System.Collections.Generic;
using System.Linq;
using APTAT.Core.Reports;

namespace APTAT.Core.Descent.Apt
{
    public static class ScanResultExt
    {
        public static bool IsLookupInProgress(this CloudVerdict verdict)
        {
            return verdict == CloudVerdict.FailedToAnalyze || verdict == CloudVerdict.NoVerdict;
        }

        public static bool IsUnknown(this CloudVerdict verdict)
        {
            return verdict == CloudVerdict.Unknown|| verdict == CloudVerdict.FirstSeen;
        }

        public  static DetailedBadFile[] MapToBusiness(this IEnumerable<IModule> files, ScannedTarget scannedTarget)
        {
            
            if (files == null) return new DetailedBadFile[] { };
            var q = from module in files
                where !module.Sha1.StartsWith("0000000000000000000000000000000000000000")
                select new DetailedBadFile(module, scannedTarget);
            return q.ToArray();

        }
    }
}