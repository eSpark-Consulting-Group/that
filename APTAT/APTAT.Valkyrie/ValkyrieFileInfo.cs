using System;
using System.Diagnostics;
using System.Net;
using APTAT.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace APTAT.Valkyrie
{
    public class ValkyrieFileInfo
    {
        public ValkyrieFileInfo()
        {
            PreviousValkyrieVerdict = ValkyrieVerdict.Pending;
            Paths = new ReadOnlyCollection<string>(_pathList);
        }
        public string Path { get; set; }
        private readonly List<string> _pathList = new List<string>();
        public ReadOnlyCollection<string> Paths { get; }

        public string Sha1 { get; set; }
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
                return Path.Replace(":", "$");
            return $"\\\\{GetMachineName()}\\{Path.Replace(":", "$")}";
        }
        public override int GetHashCode()
        {
            return Sha1.GetHashCode();
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
