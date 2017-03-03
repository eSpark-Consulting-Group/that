using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Core.Storage
{
    public static class CloudVerdictMapper
    {
        public static Descent.Apt.CloudVerdict ToBusiness(this DAL.Enums.CloudVerdict cloudVerdict)
        {
            switch (cloudVerdict)
            {
                case DAL.Enums.CloudVerdict.Safe:
                    return Descent.Apt.CloudVerdict.Safe;
                case DAL.Enums.CloudVerdict.Unknown:
                    return Descent.Apt.CloudVerdict.Unknown;
                case DAL.Enums.CloudVerdict.Malware:
                    return Descent.Apt.CloudVerdict.Malware;
                case DAL.Enums.CloudVerdict.FirstSeen:
                    return Descent.Apt.CloudVerdict.FirstSeen;
                case DAL.Enums.CloudVerdict.FailedToAnalyze:
                    return Descent.Apt.CloudVerdict.FailedToAnalyze;
                case DAL.Enums.CloudVerdict.NoVerdict:
                    return Descent.Apt.CloudVerdict.NoVerdict;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cloudVerdict));
            }
        }

        public static DAL.Enums.CloudVerdict ToData(this Descent.Apt.CloudVerdict cloudVerdict)
        {
            switch (cloudVerdict)
            {
                case Descent.Apt.CloudVerdict.Safe:
                    return DAL.Enums.CloudVerdict.Safe;

                case Descent.Apt.CloudVerdict.Unknown:
                    return DAL.Enums.CloudVerdict.Unknown;

                case Descent.Apt.CloudVerdict.Malware:
                    return DAL.Enums.CloudVerdict.Malware;

                case Descent.Apt.CloudVerdict.FirstSeen:
                    return DAL.Enums.CloudVerdict.FirstSeen;

                case Descent.Apt.CloudVerdict.FailedToAnalyze:
                    return DAL.Enums.CloudVerdict.FailedToAnalyze;

                case Descent.Apt.CloudVerdict.NoVerdict:
                    return DAL.Enums.CloudVerdict.NoVerdict;

                default:
                    throw new ArgumentOutOfRangeException(nameof(cloudVerdict));

            }
        }
    }
}
