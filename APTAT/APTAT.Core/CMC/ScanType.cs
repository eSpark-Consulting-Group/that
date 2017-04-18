namespace APTAT.Core.CMC
{
    public enum ScanType
    {
        IpScan,
        WorkgroupScan,
        ADScan,
        LocalScan,
        AutoDiscovery
    }
    public enum ScanTriggerType
    {
        ScanNow,
        CustomScan
    }
    public enum ScanTarget
    {
        TargetsWereDiscoveredByIPAddresses,
        DiscoveredByWorkGroup,
        DiscoveredByActiveDirectory,
        DiscoveredByLocalComputer,
        UsedAutoDiscoveredTargets
    }
    public enum ScanStatus
    {
        Success,
        Interrupted,
        Failed
    }
    public enum ReportKind
    {
        Executive,
        PerDevice,
        PerProgram
    }
    public enum ReportAction
    {
        Saved,
        Printed
    }
    public enum DiscoveryStatus
    {
        Success,
        Skipped,
        Failed
    }
}
