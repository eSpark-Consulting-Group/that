namespace APTAT.DeploymentCore.Directory
{
    public interface IDevicesSourceDescription
    {
        string Description { get; set; }
        ETargetsDiscovererType Type { get; set; }
    }
}