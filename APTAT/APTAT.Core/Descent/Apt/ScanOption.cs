namespace APTAT.Core.Descent.Apt
{
    public class ScanOptions
    {
        public ScanOptions(string tcpPort, string udpPort, ScanKind scanKind)
        {
            ScanPaths = new string[] { };
            TcpPort = tcpPort;
            UdpPort = udpPort;
            ScanKind = scanKind;
        }

        public ScanOptions(string tcpPort, string udpPort, string[] paths, bool scanCitical)
            :this(tcpPort, udpPort, ScanKind.Custom)
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
        //}

        public string TcpPort { get; private set; }
        public string UdpPort { get; private set; }
        public string[] ScanPaths { get; private set; }
        public bool ScanProcesses { get; private set; }
        public bool ScanDrivers { get; private set; }
        public bool ScanAutoruns { get; private set; }

        //return @"processes|drivers|autoruns"; 

        public ScanKind ScanKind { get; private set; }
    }
}