namespace APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress
{
	public class SingleIPAddress : BaseEndpointAddress
	{
		public string IPAddress { get; set; }

		public override string ToString()
		{
			return IPAddress;
		}
	}
}
