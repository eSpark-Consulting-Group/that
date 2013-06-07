using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace APTAT.Core
{
    public class ScanResult
    {
        public string HostName { get; set; }
        public Guid TargetUid { get; set; }
        public string FileName { get; set; }
        public int TotalFilesCount { get; set; }
        public int UnknownFilesCount { get; set; }

        public List<UnknownFile> UnknownFiles 
        { 
            get
            {
                if(AptReport != null)
                {
                    if(AptReport.Modules != null)
                    {
                        var unknowns = from file in AptReport.Modules
                                      where file.Cloud == CloudVerdict.Unknown
                                      select new UnknownFile() {Name = file.Path, Size = file.Size};
                        return new List<UnknownFile>(unknowns);
                    }
                }

                return new List<UnknownFile>();
            }
        }
        public AptReport AptReport { get; set; }
    }

    public class UnknownFile
    {
        public string Name { get; set; }
        public long Size { get; set; }
    }

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

        [XmlAttribute("Os")]
        public string Os { get; set; }

        [XmlArray("Connections")]
        [XmlArrayItem("Connection")]
        public Connection[] Connections { get; set; }
    }

    [XmlType("Connection")]
    public class Connection
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Type")]
        public string Type { get; set; }

        [XmlAttribute("Ip")]
        public string Ip { get; set; }
    }

    public enum CloudVerdict
    {
        [XmlEnum("safe")]
        Safe,

        [XmlEnum("unknown")]
        Unknown,

        [XmlEnum("malware")]
        Malware,
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
    }

    [XmlType("Process")]
    public class Process
    {
        [XmlAttribute("Pid")]
        public string Pid { get; set; }

        [XmlAttribute("ModuleRefId")]
        public string ModuleId { get; set; }

        [XmlAttribute("State")]
        public string State { get; set; }

        [XmlAttribute("User")]
        public string User { get; set; }

        [XmlAttribute("Created")]
        public string Created { get; set; }

        [XmlAttribute("Cmd")]
        public string CommandLine { get; set; }

        [XmlAttribute("CpuTime")]
        public string CpuTime { get; set; }

        [XmlAttribute("Platform")]
        public string Platform { get; set; }

        [XmlAttribute("Elevated")]
        public bool Elevated { get; set; }

        [XmlAttribute("UacVirtualization")]
        public string UacVirtualization { get; set; }

        [XmlArray("Modules")]
        [XmlArrayItem("Module")]
        public ModuleRef[] ModuleReferences { get; set; }

        [XmlArray("Children")]
        [XmlArrayItem("Process")]
        public Process[] Children { get; set; }
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
}
