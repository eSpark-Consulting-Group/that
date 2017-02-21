namespace APTAT.Core.Descent.Apt
{
    public class ScanOptions : IScanOptions
    {
        public ScanOptions(string tcpPort, string udpPort, ScanKind scanKind, string login, string password, bool isRescan = false)
        {
            ScanPaths = new string[] { };
            TcpPort = tcpPort;
            UdpPort = udpPort;
            Login = login;
            Password = password;
            IsRescan = isRescan;
            ScanKind = scanKind;
        }

        public ScanOptions(string tcpPort, string udpPort, string[] paths, bool scanCitical, string login, string password, bool isRescan = false)
            :this(tcpPort, udpPort, ScanKind.Custom, login, password, isRescan)
        {            
            ScanPaths = paths;
            ScanProcesses = scanCitical;
            ScanDrivers = scanCitical;
            ScanAutoruns = scanCitical;            
        }

        //public static ScanOptions CreateCustomScanOptions(string tcpPort, string udpPort, string[] paths,
        //    bool scanProcesses, bool scanDrivers, bool scanAutoruns)
        //{
        //    var s = new ScanOptions(tcpPort, udpPort, ScanKind.Custom);
        //    s.ScanPaths = paths;
        //    s.ScanProcesses = scanProcesses;
        //    s.ScanDrivers = scanDrivers;
        //    s.ScanAutoruns = scanAutoruns;
        //    return s;
        //}IEnumerable<DeploymentTarget> targets, string login, string password, bool rescanAttempt
        public bool IsRescan { get; }
        public string Password { get;  }
        public string Login { get; }
        public string TcpPort { get; private set; }
        public string UdpPort { get; private set; }
        public string[] ScanPaths { get; private set; }
        public bool ScanProcesses { get; private set; }
        public bool ScanDrivers { get; private set; }
        public bool ScanAutoruns { get; private set; }

        //return @"processes|drivers|autoruns"; 

        public ScanKind ScanKind { get; private set; }

        public bool ScanInit { get; set; }
    }
}