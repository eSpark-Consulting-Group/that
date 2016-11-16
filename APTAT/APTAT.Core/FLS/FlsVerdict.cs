using System;
using System.Runtime.CompilerServices;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core
{
    public static class CloudVerdictMap
    {
       
        internal static Core.Descent.Apt.CloudVerdict ToCloudVerdict(this FlsVerdict verdict)
        {
            switch (verdict)
            {
                case FlsVerdict.DoesntExist:
                    return CloudVerdict.FirstSeen;
                    
                case FlsVerdict.Safe:
                    return CloudVerdict.Safe;

                case FlsVerdict.Malicious:
                    return CloudVerdict.Malware;
                    
                case FlsVerdict.Unknown:
                    return CloudVerdict.Unknown;
                    
                case FlsVerdict.Error:
                    return CloudVerdict.FailedToAnalyze;
                    
                default:
                    throw new ArgumentOutOfRangeException(nameof(verdict), verdict, null);
            }
        }
    }

    internal enum FlsVerdict
    {
        DoesntExist = 0,
        Safe = 1,
        Malicious = 2,
        Unknown = 3,
        Error = 4 // if fail (means can't connect to CAMDPAMS)
    }
}