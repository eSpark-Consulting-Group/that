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

        public  static ScanModule[] MapToBusiness(this IEnumerable<IModule> files)
        {
            
            if (files == null) return new ScanModule[] { };
            var q = from module in files
                where !module.Sha1.StartsWith("0000000000000000000000000000000000000000")
                select module.Map();
            return q.ToArray();
        }
    }
}