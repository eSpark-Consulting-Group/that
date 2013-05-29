using APTAT.Core.Facade.Descent.MessageContracts;
using Esm.Server.Facade.Descent.Commands;

namespace APTAT.Core.Facade.Descent.Commands
{
	public class EnumerateWorkgroupsCommand : ResponseServiceCommand<EnumerateWorkgroupsResponse>
	{
		public EnumerateWorkgroupsCommand()
			: base()
		{
		}

		public override void Execute()
		{
			var directory = SubSystemFactory.Current.GetSubSystem<IDirectorySubSystem>();
			var networkObserver = directory as INetworkObserver;
			string[] workgroups = networkObserver.EnumerateWorkgroups();
			Response.Workgroups = workgroups;
			Response.IsCompleted = true;
		}
	}
}
