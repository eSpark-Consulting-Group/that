using System.Collections.Generic;

namespace APTAT.Core.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress
{
	public class EndpointAddressCollection : List<BaseEndpointAddress>
	{
	    public EndpointAddressCollection()
	    {
	    }

	    public EndpointAddressCollection(int capacity)
            : base(capacity)
	    {
	    }

	    public EndpointAddressCollection(IEnumerable<BaseEndpointAddress> collection)
            : base(collection)
	    {
	    }
	}
}
