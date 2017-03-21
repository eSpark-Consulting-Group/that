namespace APTAT.Core.Descent.Apt
{
    public interface IScanOptions
    {
        bool IsRescan { get; }
        string Login { get; }
        string Password { get; }
        bool ScanAutoruns { get; }
        bool ScanDrivers { get; }
        bool ScanInit { get; set; }
        ScanKind ScanKind { get; }
        string[] ScanPaths { get; }
        bool ScanProcesses { get; }
        int TcpPort { get; }
        int UdpPort { get; }
    }
}