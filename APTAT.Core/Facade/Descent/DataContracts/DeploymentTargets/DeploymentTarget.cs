using System;

namespace APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets
{
	public class DeploymentTarget
	{
		public Guid TargetUid { get; set; }

		public string Name { get; set; }

		public string DnsName { get; set; }

		public bool Online { get; set; }

		public bool Installed { get; set; }

		public string IPAddress { get; set; }

		public bool IsEsmServer { get; set; }

		#if DEBUG
		public override string ToString()
		{
			return String.Format("Name='{0}';DnsName='{1}';IPAddress='{2}';Online={3};Installed={4}", Name, DnsName, IPAddress, Online, Installed);
		}
		#endif
	}
}
