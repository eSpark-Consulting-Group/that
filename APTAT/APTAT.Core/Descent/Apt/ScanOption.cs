namespace APTAT.Core.Descent.Apt
{
    public class ScanOptions
    {
        public ScanOptions(string tcpPort, string udpPort, string[] paths, ScanKind kind)
        {
            TcpPort = tcpPort;
            UdpPort = udpPort;
            ScanPaths = paths;
            ScanKind = kind;
        }
        public string TcpPort { get; private set; }
        public string UdpPort { get; private set; }
        public string[] ScanPaths { get; private set; }
        public ScanKind ScanKind { get; private set; }
    }
}