using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Core.Storage
{
    public static class ScanKindMapper
    {
        public static DAL.Enums.ScanKind ToData(this APTAT.Core.Descent.Apt.ScanKind scanKind)
        {
            switch (scanKind)
            {
                case Descent.Apt.ScanKind.Quick:
                    return DAL.Enums.ScanKind.Quick;

                case Descent.Apt.ScanKind.Full:
                    return DAL.Enums.ScanKind.Full;

                case Descent.Apt.ScanKind.Custom:
                    return DAL.Enums.ScanKind.Custom;

                default:
                    throw new ArgumentOutOfRangeException(nameof(scanKind));
            }
        }

        public static Descent.Apt.ScanKind ToBusiness(this DAL.Enums.ScanKind scanKind)
        {
            switch (scanKind)
            {
                case DAL.Enums.ScanKind.Quick:
                    return Descent.Apt.ScanKind.Quick;
                case DAL.Enums.ScanKind.Full:
                    return Descent.Apt.ScanKind.Full;
                case DAL.Enums.ScanKind.Custom:
                    return Descent.Apt.ScanKind.Custom;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scanKind));
            }
        }
    }
}
