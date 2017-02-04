using System;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core
{
    public static class FlsResultExt
    {
        public static Boolean IsNotAnalyzed(this IFlsResult result)
        {
            return result.Cloud == CloudVerdict.FailedToAnalyze || result.Cloud == CloudVerdict.NoVerdict;
        }
        public static Boolean IsUnknown(this IFlsResult result)
        {
            return result.Cloud == CloudVerdict.Unknown || result.Cloud == CloudVerdict.FirstSeen;
        }
    }

    public interface IFlsResult
    {
        string Sha1 { get;  }
        CloudVerdict Cloud { get; set; }
    }
}