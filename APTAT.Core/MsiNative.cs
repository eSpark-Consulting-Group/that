using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace APTAT.Core
{
    public static class MsiNative
    {
        public const uint ERROR_MORE_DATA = 234;

        public static readonly string INSTALLPROPERTY_PUBLISHER = "Publisher";

        public static readonly string INSTALLPROPERTY_VERSION = "Version";
        public static readonly string INSTALLPROPERTY_VERSIONSTRING = "VersionString";
        public static readonly string INSTALLPROPERTY_PRODUCTVERSION = "ProductVersion";

        public static readonly string INSTALLPROPERTY_PRODUCTCODE = "ProductCode";
        public static readonly string INSTALLPROPERTY_UPGRADECODE = "UpgradeCode";
        public static readonly string INSTALLPROPERTY_PRODUCTNAME = "ProductName";

        public enum UILevel : uint
        {
            INSTALLUILEVEL_NONE = 2
        }


        public enum INSTALLMESSAGE
        {
            INSTALLMESSAGE_FATALEXIT = 0x00000000, // premature termination, possibly fatal OOM
            INSTALLMESSAGE_ERROR = 0x01000000, // formatted error message
            INSTALLMESSAGE_WARNING = 0x02000000, // formatted warning message
            INSTALLMESSAGE_USER = 0x03000000, // user request message
            INSTALLMESSAGE_INFO = 0x04000000, // informative message for log
            INSTALLMESSAGE_FILESINUSE = 0x05000000, // list of files in use that need to be replaced
            INSTALLMESSAGE_RESOLVESOURCE = 0x06000000, // request to determine a valid source location
            INSTALLMESSAGE_OUTOFDISKSPACE = 0x07000000, // insufficient disk space message
            INSTALLMESSAGE_ACTIONSTART = 0x08000000, // start of action: action name & description
            INSTALLMESSAGE_ACTIONDATA = 0x09000000, // formatted data associated with individual action item
            INSTALLMESSAGE_PROGRESS = 0x0A000000, // progress gauge info: units so far, total
            INSTALLMESSAGE_COMMONDATA = 0x0B000000, // product info for dialog: language Id, dialog caption
            INSTALLMESSAGE_INITIALIZE = 0x0C000000, // sent prior to UI initialization, no string data
            INSTALLMESSAGE_TERMINATE = 0x0D000000, // sent after UI termination, no string data
            INSTALLMESSAGE_SHOWDIALOG = 0x0E000000 // sent prior to display or authored dialog or wizard
        }

        public enum INSTALLLOGMODE  // bit flags for use with MsiEnableLog and MsiSetExternalUI
        {
            INSTALLLOGMODE_FATALEXIT = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_FATALEXIT >> 24)),
            INSTALLLOGMODE_ERROR = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_ERROR >> 24)),
            INSTALLLOGMODE_WARNING = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_WARNING >> 24)),
            INSTALLLOGMODE_USER = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_USER >> 24)),
            INSTALLLOGMODE_INFO = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_INFO >> 24)),
            INSTALLLOGMODE_RESOLVESOURCE = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_RESOLVESOURCE >> 24)),
            INSTALLLOGMODE_OUTOFDISKSPACE = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_OUTOFDISKSPACE >> 24)),
            INSTALLLOGMODE_ACTIONSTART = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_ACTIONSTART >> 24)),
            INSTALLLOGMODE_ACTIONDATA = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_ACTIONDATA >> 24)),
            INSTALLLOGMODE_COMMONDATA = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_COMMONDATA >> 24)),
            INSTALLLOGMODE_PROPERTYDUMP = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_PROGRESS >> 24)), // log only
            INSTALLLOGMODE_VERBOSE = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_INITIALIZE >> 24)), // log only
            INSTALLLOGMODE_EXTRADEBUG = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_TERMINATE >> 24)), // log only
            INSTALLLOGMODE_LOGONLYONERROR = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_SHOWDIALOG >> 24)), // log only    
            INSTALLLOGMODE_PROGRESS = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_PROGRESS >> 24)), // external handler only
            INSTALLLOGMODE_INITIALIZE = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_INITIALIZE >> 24)), // external handler only
            INSTALLLOGMODE_TERMINATE = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_TERMINATE >> 24)), // external handler only
            INSTALLLOGMODE_SHOWDIALOG = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_SHOWDIALOG >> 24)), // external handler only
            INSTALLLOGMODE_FILESINUSE = (1 << (INSTALLMESSAGE.INSTALLMESSAGE_FILESINUSE >> 24)), // external handler only
        }

        public enum INSTALLLOGATTRIBUTES // flag attributes for MsiEnableLog
        {
            INSTALLLOGATTRIBUTES_APPEND = (1 << 0),
            INSTALLLOGATTRIBUTES_FLUSHEACHLINE = (1 << 1),
        }

        [DllImport("msi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern UInt32 MsiEnableLog(INSTALLLOGMODE dwLogMode, string szLogFile, INSTALLLOGATTRIBUTES dwLogAttributes);

        [DllImport("msi.dll", SetLastError = true)]
        public static extern int MsiSetInternalUI(UILevel dwUILevel, ref IntPtr phWnd);

        [DllImport("msi.dll", SetLastError = true)]
        public static extern UInt32 MsiOpenPackage(string path, out IntPtr hProduct);

        [Flags]
        public enum OpenFlags : uint
        {
            MSIOPENPACKAGEFLAGS_IGNOREMACHINESTATE = 1
        }
        [DllImport("msi.dll", SetLastError = true)]
        public static extern UInt32 MsiOpenPackageEx(string path, OpenFlags flags, out IntPtr hProduct);

        [DllImport("msi.dll", SetLastError = true)]
        public static extern UInt32 MsiGetProductCode(IntPtr hProduct, [Out] StringBuilder valueBuf);

        [DllImport("msi.dll", SetLastError = true)]
        public static extern UInt32 MsiGetProductProperty(IntPtr hProduct, string property, [Out] StringBuilder valueBuf, ref UInt32 len);

        [DllImport("msi.dll", SetLastError = true)]
        public static extern UInt32 MsiOpenProduct(string productCode, out IntPtr hProduct);

        [DllImport("msi.dll", SetLastError = true)]
        public static extern UInt32 MsiGetProductInfo(string productCode, string property, [Out] StringBuilder valueBuf, ref UInt32 len);

        [DllImport("msi.dll", ExactSpelling = true)]
        public static extern UInt32 MsiCloseHandle(IntPtr hAny);

        [DllImport("msi")]
        public static extern uint MsiGetSummaryInformation(IntPtr dbhandle, string path, uint updateCount,
            out IntPtr infoHandle);

        [DllImport("msi")]
        public static extern uint MsiSummaryInfoGetProperty(
            IntPtr infoHandle, uint property, out IntPtr dataType,
            out IntPtr value, out System.Runtime.InteropServices.ComTypes.FILETIME fileTime, StringBuilder lpValueBuf, ref int pcchValueBuf);
    }
}
