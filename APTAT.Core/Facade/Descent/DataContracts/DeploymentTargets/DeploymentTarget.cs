using System;

namespace APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets
{
	public class DeploymentTarget
	{
		public Guid TargetUid { get; set; }

		public string Name { get; set; }

		public string DnsName { get; set; }

		public bool Online { get; set; }

		public string IPAddress { get; set; }

        public string LdapPath { get; set; }
	}
}
