using System;
using System.Diagnostics;
using System.Net;
using APTAT.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace APTAT.Valkyrie
{
    public class ValkyrieFileInfo : IValkyrieFileInfo
    {
        public ValkyrieFileInfo(string sha1, string iP, string path, long size, CloudVerdict cloud, 
            bool isLocal, ValkyrieVerdict previousVerdict)
        {
            Sha1 = sha1;
            PreviousValkyrieVerdict = previousVerdict;
            Paths = new ReadOnlyCollection<string>(_pathList);
            Ip = iP;
            Path = path;
            Size = size;
            FlsCloud = cloud;
            IsLocalComputer = isLocal;
        }

        public string Path { get; set; }
        private readonly List<string> _pathList = new List<string>();
        public ReadOnlyCollection<string> Paths { get; }

        public string Sha1 { get; }
        public string Ip { get; set; }     
        public CloudVerdict FlsCloud { get; set; }
        public ValkyrieVerdict PreviousValkyrieVerdict { get; set; }

        public string GetFullPath()
        {
            if (Path.StartsWith("\\\\"))
                return Path;
            return $"\\\\{Ip}\\{Path}";
        }

        public void AddFullPath(string ip, string path)
        {
            if (Path.StartsWith("\\\\"))
                _pathList.Add(path);
            _pathList.Add( $"\\\\{ip}\\{path}");
        }                

        public long Size { get; set; }
        public bool IsLocalComputer { get; set; }

        public string GetNetworkPath()
        {
            if (Path.StartsWith("\\\\"))
            {
                if (IsLocalComputer)
                {
                    var p = Path.TrimStart(new[] {'\\'});
                    return p.Substring(p.IndexOf('\\') + 1).Replace("$", ":");
                }
                return Path.Replace(":", "$");
            }
            if (IsLocalComputer)
                return Path;
            return $"\\\\{GetMachineName()}\\{Path.Replace(":", "$")}";
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
                IPHostEntry hostEntry = Dns.GetHostEntry(Ip);

                machineName = hostEntry.HostName;
            }
            catch (Exception ex)
            {
                LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network, $"{ex.GetType()} occurred during getting host name for {Ip}");
                LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network, ex.ToString());
                machineName = Ip;
                // Machine not found...
            }
            return machineName;
        }
    }
}
