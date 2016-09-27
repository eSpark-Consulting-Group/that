using System;
using System.Diagnostics;
using System.Net;
using APTAT.Common;


namespace APTAT.Valkyrie
{
    public class ValkyrieFileInfo
    {
        public string Path { get; set; }
        public string Sha1 { get; set; }
        public string Ip { get; set; }     
        public CloudVerdict FlsCloud { get; set; }
        public ValkyrieVerdict PreviousValkyrieVerdict { get; set; }

        public string GetFullPath()
        {
            return String.Format("\\\\{0}\\{1}", Ip, Path);
        }

        public long Size { get; set; }
        public bool IsLocalComputer { get; set; }

        public string GetNetworkPath()
        {

            return String.Format("\\\\{0}\\{1}", GetMachineName(), Path.Replace(":", "$"));
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
