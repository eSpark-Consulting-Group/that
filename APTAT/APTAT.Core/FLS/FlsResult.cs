using System;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core
{
    public class FlsResult:IFlsResult
    {
        //public FlsVerdict FlsVerdict { get; private set; }

        public CloudVerdict Cloud { get; set; }

        public bool IsNotAnalyzed => Cloud == CloudVerdict.FailedToAnalyze || Cloud == CloudVerdict.NoVerdict;

        //public string FlsVerdictString
        //{
        //    get
        //    {
        //        switch (Cloud)
        //        {
        //            case CloudVerdict.FirstSeen:
        //                return "Does not exist";

        //            case CloudVerdict.Safe:
        //                return "Safe";

        //            case CloudVerdict.Malware:
        //                return "Malicious";

        //            case CloudVerdict.Unknown:
        //                return "Unknown";

        //            default:
        //                throw new ArgumentOutOfRangeException();
        //        }
        //    }
        //}

     //   public byte CamasVerdict { get; set; }
        public string Sha1 { get; set; }
    }
}


