using System;

namespace APTAT.Core.Descent.Wmi
{
    public class WmiScanResult
    {
        public MainboardInfo MainboardInfo { get; set; }
        public Guid TargetUid { get; set; }
        public string HostName { get; set; }
        public string IpAddress { get; set; }
        public string OrganizationUnit { get; set; }
        public ScanResultType ResultType { get; set; }
        public string ResultErrorText { get; set; }
        public int ProgressPercenage { get; set; }


    }
    public class MainboardInfo
    {
        public string SMBIOSBIOSVersion { get; set; }
        public string ReleaseDate { get; set; }
        public string Manufacturer { get; set; }
    }

}
