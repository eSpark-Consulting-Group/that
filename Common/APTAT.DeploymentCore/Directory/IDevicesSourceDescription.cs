namespace APTAT.DeploymentCore.Directory
{
    public interface IDevicesSourceDescription
    {
        object Description { get; set; }
        ETargetsDiscovererType Type { get; set; }
    }
}