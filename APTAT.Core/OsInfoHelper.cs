using APTAT.Core.Descent.Apt;

namespace APTAT.Core
{
    public static class OsInfoHelper
    {
        public enum ProductType
        {
            Workstation = 1,
            DomainController = 2,
            Server = 3
        }

        public enum SuiteMask
        {
            WhServer = 0x00008000
        }

        public static string GetWindowsDisplayString(int osMajorVersion, int osMinorVersion, ProductType productType
                                                    , bool isServerR2, int suteMask
                                                    , int spMajorVersion, int spMinorVersion, OsPlatform bitSystem)
        {
            string osName = "Unknown";

            if (osMajorVersion == 5)
            {
                switch (osMinorVersion)
                {
                    case 0:
                        osName = "Windows 2000";
                        break;
                    case 1:
                        osName = "Windows XP";
                        break;
                    case 2:
                        if (suteMask == (int)SuiteMask.WhServer)
                        {
                            osName = "Windows Home Server";
                        }
                        else if (productType == ProductType.Workstation)
                        {
                            osName = "Windows XP";
                        }
                        else if (isServerR2)
                        {
                            osName = "Windows Server 2003 R2";
                        }
                        else
                        {
                            osName = "Windows Server 2003";
                        }
                        break;

                }
            }
            else if (osMajorVersion == 6)
            {
                switch (osMinorVersion)
                {
                    case 0:
                        osName = productType == ProductType.Workstation
                                     ? "Windows Vista"
                                     : "Windows Server 2008";
                        break;
                    case 1:
                        osName = productType == ProductType.Workstation
                                     ? "Windows 7"
                                     : "Windows Server 2008 R2";
                        break;
                    case 2:
                        osName = productType == ProductType.Workstation
                                     ? "Windows 8"
                                     : "Windows Server 2012";
                        break;
                    case 3:
                        osName = productType == ProductType.Workstation
                                     ? "Windows 8.1"
                                     : "Windows Server 2012 R2";
                        break;
                }
            }
            else if (osMajorVersion == 10)
            {
                osName = "Windows 10";
            }


            string bitSystemStr = ConvertBitSystemToDisplay(bitSystem);
            string sp = ConvertSPToDisplay(spMajorVersion, spMinorVersion);


            return string.Format("{0}{1}{2}", osName, bitSystemStr, sp);
        }

        private static string ConvertBitSystemToDisplay(OsPlatform bitSystem)
        {
            return bitSystem == OsPlatform.X64 ? "(x64)" : "(x86)";
        }

        private static string ConvertSPToDisplay(int spMajorVersion, int spMinorVersion)
        {
            if (spMajorVersion == 0)
                return string.Empty;

            if (spMinorVersion == 0)
            {
                return string.Format("SP{0}", spMajorVersion);
            }
            else
            {
                return string.Format("SP{0}.{1}", spMajorVersion, spMinorVersion);
            }
        }
    }
}
