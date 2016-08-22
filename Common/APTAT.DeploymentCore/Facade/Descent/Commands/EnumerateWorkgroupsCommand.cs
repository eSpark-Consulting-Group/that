using APTAT.DeploymentCore.Facade.Descent.MessageContracts;

namespace APTAT.DeploymentCore.Facade.Descent.Commands
{
    public class EnumerateWorkgroupsCommand : ResponseServiceCommand<EnumerateWorkgroupsResponse>
    {
        private readonly INetworkBrowser _networkBrowser;
        public EnumerateWorkgroupsCommand(INetworkBrowser networkBrowser)
        {
            _networkBrowser = networkBrowser;
        }
        public override void Execute()
        {
            string[] workgroups = _networkBrowser.EnumerateWorkgroups();
            Response.Workgroups = workgroups;
            Response.IsCompleted = true;
        }
    }
}
