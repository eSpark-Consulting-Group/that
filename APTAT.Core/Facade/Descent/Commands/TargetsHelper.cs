using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core.Facade.Descent.Commands
{
	public static class TargetsHelper
	{
        // TODO: Commented
        //public static void FindManagedTargetsAndEsmServerHost( IEnumerable<DeploymentTarget> targets )
        //{
        //    var storage = SubSystemFactory.Current.GetSubSystem<IEntityStorageSubsystem>();
        //    Computer[] computers;
        //    using (var unit = storage.Storage.GetContext())
        //    {
        //        computers = unit.ComputerRepository.GetAllVisible(cacheEntitiesInContext: false);
        //    }

        //    var serverDnsName = NetHelp.GetLocalHostName(true);
        //    var serverAddresses = NetHelp.GetSupportedExternalAddresses(true);
			
        //    foreach( var target in targets )
        //    {
        //        var t = target; // To avoid "Access to modified closure" warning
        //        t.Installed = computers.Any(c => c.IPAddress == t.IPAddress &&
        //                                         String.Equals(c.DnsName, t.DnsName, StringComparison.InvariantCultureIgnoreCase));

        //        t.IsEsmServer = serverAddresses.Any(ip => ip == t.IPAddress) ||
        //                        String.Equals(t.DnsName, serverDnsName, StringComparison.InvariantCultureIgnoreCase);
        //    }
        //}

		public static void FindOnlineTargets( INetworkObserver networkObserver, IEnumerable<DeploymentTarget> computers )
		{
			//TODO: Check duplcated keys.
			var computersDict = computers.ToDictionary( target => target.Name );
			using( CountdownEvent countdownEvent = new CountdownEvent( computersDict.Count ) )
			{
				Parallel.ForEach(
					computersDict.Values,
					computer => networkObserver.SendPingAsync(computer.Name,
						(pingRes, ipAddress) =>
						{
							computersDict[computer.Name].Online = pingRes == IPStatus.Success;
							countdownEvent.Signal();
						} ) );

				countdownEvent.Wait();
			}
		}

	}
}
