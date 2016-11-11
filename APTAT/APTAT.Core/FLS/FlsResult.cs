using System;

namespace APTAT.Core
{
    public class FlsResult
    {
        public FlsVerdict FlsVerdict { get; set; }
        public string FlsVerdictString
        {
            get
            {
                switch (FlsVerdict)
                {
                    case FlsVerdict.DoesntExist:
                        return "Does not exist";

                    case FlsVerdict.Safe:
                        return "Safe";

                    case FlsVerdict.Malicious:
                        return "Malicious";

                    case FlsVerdict.Unknown:
                        return "Unknown";

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public byte CamasVerdict { get; set; }
        public string Sha1 { get; set; }
    }
}


