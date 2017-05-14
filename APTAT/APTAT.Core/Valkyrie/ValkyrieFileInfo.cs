using System;
using System.Diagnostics;
using System.Net;
using APTAT.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore;
using APTAT.Core.Descent.Copy;
using APTAT.Core.DescentCopy;

namespace APTAT.Core.Valkyrie
{
    public class ValkyrieFileInfo : IValkyrieFileInfo
    {
        public ValkyrieFileInfo(string sha1, IComputer computer, string path, long size, CloudVerdict cloud, 
            ValkyrieVerdict previousVerdict)
        {
            Sha1 = sha1;
            PreviousValkyrieVerdict = previousVerdict;
            Instances = new ReadOnlyCollection<IFileInstanceInformation>(_instanceList);
            Path = path;
            Size = size;
            FlsCloud = cloud;
            Computer = computer;
        }
        public IComputer Computer { get; }
        public string Path { get; set; }

        private readonly List<IFileInstanceInformation> _instanceList = new List<IFileInstanceInformation>();
        public ReadOnlyCollection<IFileInstanceInformation> Instances { get; }

        public string Sha1 { get; }    
        public CloudVerdict FlsCloud { get; set; }
        public ValkyrieVerdict PreviousValkyrieVerdict { get; set; }

        public IEnumerable<string> FilePathes
        {
            get
            {
                foreach(var instance in Instances)
                {
                    if (instance.Path.StartsWith("\\\\"))
                        yield return instance.Path;
                    else
                        yield return $@"\\{instance.Computer.IPAddress}\{instance.Path}";
                }
            }
        }

        public string GetFullPath()
        {
            if (Path.StartsWith("\\\\"))
                return Path;
            return $"\\\\{Computer.IPAddress}\\{Path}";
        }

        public void AddFullPath(IComputer computer, string path)
        {
            _instanceList.Add(new FileInstanceInformation(computer, path));
        }                

        public long Size { get; set; }
        public ICopyFileResult CopyResult
        {
            get;set;
        }

        public string GetNetworkPath(string path = null)
        {
            if (path == null)
                path = Path;
            if (path.StartsWith("\\\\"))
            {
                if (Computer.IsLocalComputer)
                {
                    var p = path.TrimStart(new[] {'\\'});
                    return p.Substring(p.IndexOf('\\') + 1).Replace("$", ":");
                }
                return path.Replace(":", "$");
            }
            if (Computer.IsLocalComputer)
                return path;
            return $"\\\\{GetMachineName()}\\{path.Replace(":", "$")}";
        }
        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Sha1);
        }

        public override bool Equals(object obj)
        {
            ValkyrieFileInfo otherFileInfo = obj as ValkyrieFileInfo;
            return otherFileInfo != null && otherFileInfo.Sha1 == Sha1;
        }
        public string GetMachineName()
        {
            string machineName = string.Empty;
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(Computer.IPAddress);

                machineName = hostEntry.HostName;
            }
            catch (Exception ex)
            {
                LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network, $"{ex.GetType()} occurred" +
                    $" during getting host name for {Computer.IPAddress}");
                LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network, ex.ToString());
                machineName = Computer.IPAddress;
                // Machine not found...
            }
            return machineName;
        }

        public class FileInstanceInformation : IFileInstanceInformation
        {
            public FileInstanceInformation(IComputer computer, string path)
            {
                Path = path;
                Computer = computer;
            }
            public IComputer Computer { get; }

            public string Path { get; }
        }
    }
}
