namespace APTAT.Core
{
    public interface IProductManagementSubSystem : ISubSystem
    {
        string GetEsmCurrentVersion();
        string GetAgentVersion(SystemKind agentKind);
        Package GetAgentPackage(SystemKind systemKind);
    }
}
