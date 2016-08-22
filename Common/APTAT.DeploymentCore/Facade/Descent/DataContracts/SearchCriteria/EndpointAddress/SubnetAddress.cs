using System;

namespace APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress
{
	public class SubnetAddress : BaseEndpointAddress
	{
		public string SubnetMask { get; set; }

		public string IPAddress { get; set; }

		public override string ToString()
		{
			return String.Format( "{0} / {1}", IPAddress, SubnetMask );
		}
	}
}
