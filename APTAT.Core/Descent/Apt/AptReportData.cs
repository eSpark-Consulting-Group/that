using System;
using System.IO;
using System.Xml.Serialization;

namespace APTAT.Core.Descent.Apt
{
    [XmlRoot("AptReport")]
    public class AptReport
    {
        [XmlElement("Computer")]
        public Computer Computer { get; set; }

        [XmlArray("Modules")]
        [XmlArrayItem("Module")]
        public Module[] Modules { get; set; }

        [XmlArray("Processes")]
        [XmlArrayItem("Process")]
        public Process[] Processes { get; set; }

        [XmlArray("Drivers")]
        [XmlArrayItem("Driver")]
        public Driver[] Drivers { get; set; }

        [XmlArray("Autoruns")]
        [XmlArrayItem("Autorun")]
        public Autorun[] Autoruns { get; set; }
    }

    [XmlType("Computer")]
    public class Computer
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("SID")]
        public string Sid { get; set; }

        [XmlElement("OperatingSystem")]
        public OperatingSystem Os { get; set; }

        [XmlArray("Connections")]
        [XmlArrayItem("Connection")]
        public Connection[] Connections { get; set; }
    }

    [XmlType("OperatingSystem")]
    public class OperatingSystem
    {
        [XmlAttribute("BitSystem")]
        public OsPlatform Platform { get; set; }

        [XmlAttribute("ProductType")]
        public int ProductType { get; set; }

        [XmlAttribute("IsServerR2")]
        public bool IsServerR2 { get; set; }

        [XmlAttribute("SuiteMask")]
        public int SuiteMask { get; set; }

        [XmlAttribute("ServicePackMinor")]
        public int ServicePackMinor { get; set; }

        [XmlAttribute("ServicePackMajor")]
        public int ServicePackMajor { get; set; }

        [XmlAttribute("BuildNumber")]
        public int BuildNumber { get; set; }

        [XmlAttribute("MinorVer")]
        public int MinorVer { get; set; }

        [XmlAttribute("MajorVer")]
        public int MajorVer { get; set; }

        public override string ToString()
        {
            return OsInfoHelper.GetWindowsDisplayString(MajorVer, MinorVer, (OsInfoHelper.ProductType)ProductType, IsServerR2, SuiteMask, ServicePackMajor, ServicePackMinor, Platform);
        }
    }

    [XmlType("Connection")]
    public class Connection
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Type")]
        public string Type { get; set; }

        [XmlAttribute("IPv4")]
        public string IPv4 { get; set; }

        [XmlAttribute("IPv6")]
        public string IPv6 { get; set; }
    }

    public enum CloudVerdict
    {
        [XmlEnum("safe")]
        Safe,

        [XmlEnum("unknown")]
        Unknown,

        [XmlEnum("malware")]
        Malware,

        [XmlEnum("doesntexist")]
        FirstSeen,

        [XmlEnum("error")]
        FailedToAnalyze,

        [XmlEnum("servererror")]
        NoVerdict
    }

    public enum OsPlatform
    {
        [XmlEnum("64")]
        X64 = 64,

        [XmlEnum("32")]
        X86 = 32
    }

    [XmlType("Module")]
    public class Module
    {
        [XmlAttribute("Id")]
        public string Id { get; set; }

        [XmlAttribute("Path")]
        public string Path { get; set; }

        [XmlAttribute("Cloud")]
        public CloudVerdict Cloud { get; set; }

        [XmlAttribute("CloudDate")]
        public DateTime CloudDate { get; set; }

        [XmlAttribute("Size")]
        public long Size { get; set; }

        [XmlAttribute("Attributes")]
        public string Attributes { get; set; }

        [XmlAttribute("Created")]
        public DateTime Created { get; set; }

        [XmlAttribute("Modified")]
        public DateTime Modified { get; set; }

        [XmlAttribute("Accessed")]
        public DateTime Accessed { get; set; }

        [XmlAttribute("Sha1")]
        public string Sha1 { get; set; }

        [XmlAttribute("Signer")]
        public string Signer { get; set; }

        [XmlAttribute("Description")]
        public string Description { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }

        [XmlAttribute("Copyright")]
        public string Copyright { get; set; }

        [XmlAttribute("Product")]
        public string Product { get; set; }

        [XmlIgnore]
        public FileAttributes FileAttributes
        {
            get
            {
                Int32 flags = 0;
                try
                {
                    flags = Convert.ToInt32(Attributes ?? "0", 16);
                }
                catch (Exception)
                {
                }

                return (FileAttributes)flags;
            }
        }

        [XmlIgnore]
        public string FileSizeFormatted
        {
            get { return SizeToString(Size); }
        }

        private static string SizeToString(long size)
        {
            if (size < 1024)
                return String.Format("{0} KB ({1} bytes)", 1, size);
            else //if (size < 1024 * 1024)
                return String.Format("{0} KB ({1} bytes)", (int)(size / (double)1024), size);
            //else if(size < 1024 * 1024 * 1024)
            //    return String.Format("{0} MB", (int)(size / (double)(1024 * 1024)));
            //else
            //    return String.Format("{0} GB", (int)(size / (double)(1024 * 1024 * 1024)));
        }

        [XmlIgnore]
        public string FileAge
        {
            get { return FileAgeFormatter.GetAge(Created); }
        }

        [XmlIgnore]
        public bool IsBad
        {
            get
            {
                return Cloud == CloudVerdict.FirstSeen || Cloud == CloudVerdict.Malware || Cloud == CloudVerdict.Unknown || Cloud == CloudVerdict.FailedToAnalyze || Cloud == CloudVerdict.NoVerdict;
            }
        }

        //[XmlIgnore]
        //public Valkyrie.Client.AnalysisResult ValkyrieAnalysisResult { get; set; }


    }

    [XmlType("Process")]
    public class Process
    {
        [XmlAttribute("Pid")]
        public int Pid { get; set; }

        [XmlAttribute("ParentPid")]
        public int ParentPid { get; set; }

        [XmlAttribute("ModuleRefId")]
        public string ModuleId { get; set; }

        //[XmlAttribute("State")]
        //public string State { get; set; }

        [XmlAttribute("User")]
        public string User { get; set; }

        [XmlAttribute("Created")]
        public string Created { get; set; }

        [XmlAttribute("Cmd")]
        public string CommandLine { get; set; }

        [XmlAttribute("CpuTime")]
        public string CpuTime { get; set; }

        //[XmlAttribute("Platform")]
        //public string Platform { get; set; }

        [XmlAttribute("Elevated")]
        public bool Elevated { get; set; }

        [XmlAttribute("UacVirtualization")]
        public bool UacVirtualization { get; set; }

        [XmlArray("Modules")]
        [XmlArrayItem("Module")]
        public ModuleRef[] ModuleReferences { get; set; }

    }

    [XmlType("ModuleRef")]
    public class ModuleRef : ModuleRefBase
    {
    }

    [XmlType("Driver")]
    public class Driver : ModuleRefBase
    {
    }

    [XmlType("Autorun")]
    public class Autorun : ModuleRefBase
    {
        [XmlAttribute("Scope")]
        public string Scope { get; set; }

        [XmlAttribute("Where")]
        public string Where { get; set; }
    }

    public abstract class ModuleRefBase
    {
        [XmlAttribute("ModuleRefId")]
        public string ModuleId { get; set; }
    }


    public class FileAgeFormatter
    {
        private const int DaysInYear = 365;
        private const int DaysOnMonth = 30;
        private static readonly int[] intervalsInDays = new[] { DaysInYear, DaysOnMonth };
        private static readonly string[] oneCount = new[] { "Year", "Month" };
        private static readonly string[] moreThanOneCount = new[] { "Years", "Months" };
        private const string OneDay = "Day";
        private const string OneMonth = "Month";
        private const string OneYear = "Year";
        private const string MoreThenOneDay = "Days";
        private const string MoreThenOneMonth = "Months";
        private const string MoreThenOneYear = "Years";
        private const string OneHour = "Hour";
        private const string MoreThenOneHour = "Hours";
        private const string SpaceCharacter = " ";

        public static string GetAge(DateTime fileCreation, bool shortDate = true)
        {
            var formatted = String.Empty;
            var difference = DateTime.Now - fileCreation;
            int days = difference.Days;
            if (days == 0)
                return Formatter((int)difference.TotalHours, OneHour, MoreThenOneHour);
            for (var i = 0; i < intervalsInDays.Length; i++)
            {
                var intervalInDays = intervalsInDays[i];
                var count = days / intervalInDays;
                days %= intervalInDays;
                if (count <= 0) continue;
                formatted += Formatter(count, oneCount[i], moreThanOneCount[i]) + (days > 0 ? SpaceCharacter : string.Empty);
                if (shortDate) return formatted;
            }
            formatted += Formatter(days, OneDay, MoreThenOneDay);
            return formatted;
        }

        private static string Formatter(int count, string stringForOneCount, string stringForMoreThenOne)
        {
            if (count == 0)
                return String.Empty;
            if (count > 1)
                return String.Format("{0} {1}", count, stringForMoreThenOne);
            if (count == 1)
                return String.Format("{0} {1}", count, stringForOneCount);
            throw new ArgumentException("Incorrect input data");
        }
    }
}
