namespace APTAT.Core.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress
{
	public class DnsNameAddress : BaseEndpointAddress
	{
		public string DnsName { get; set; }

		public override string ToString()
		{
			return DnsName;
		}
	}
}
