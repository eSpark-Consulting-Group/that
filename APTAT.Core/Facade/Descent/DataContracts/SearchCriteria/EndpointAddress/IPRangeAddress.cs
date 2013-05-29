using System;

namespace APTAT.Core.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress
{
	public class IPRangeAddress : BaseEndpointAddress
	{
		public string StartAddress { get; set; }

		public string EndAddress { get; set; }

		public override string ToString()
		{
			return String.Format( "{0} - {1}", StartAddress, EndAddress );
		}
	}
}
