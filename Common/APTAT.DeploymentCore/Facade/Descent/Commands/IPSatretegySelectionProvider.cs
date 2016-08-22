using System;
using System.Net;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using APTAT.DeploymentCore.Transport;

namespace APTAT.DeploymentCore.Facade.Descent.Commands
{
	public class IPSatretegySelectionProvider : IIPSatretegySelectionProvider
	{
	    private readonly INetworkObserver _networkObserver;
	    public IPSatretegySelectionProvider(INetworkObserver networkObserver)
	    {
	        _networkObserver = networkObserver;
	    }

	    public IIPSelectionStrategy GetStrategy( BaseEndpointAddress address )
		{
            if ( address is DnsNameAddress )
			{
				var concreteAddress = ( DnsNameAddress )address;
				return new SingleNameSelectionStrategy(_networkObserver,

                    concreteAddress.DnsName );
			}
			else if( address is SingleIPAddress )
			{
				var concreteAddress = ( SingleIPAddress )address;
				return new SingleIPSelectionStrategy(
                    MapperHelper.GetIP( concreteAddress.IPAddress ) );
			}
			else if( address is SubnetAddress )
			{
				var concreteAddress = ( SubnetAddress )address;
				return new MaskIPSelectionStrategy(_networkObserver,
                    MapperHelper.GetIP( concreteAddress.IPAddress ), 
					MapperHelper.GetIP( concreteAddress.SubnetMask ) );
			}
			else if( address is IPRangeAddress )
			{
				var concreteAddress = ( IPRangeAddress )address;
				return new RangeIPSelectionStrategy(_networkObserver,
                    MapperHelper.GetIP( concreteAddress.StartAddress ), 
					MapperHelper.GetIP( concreteAddress.EndAddress ) );
			}
			else
			{
				throw new ArgumentException( "address" );
			}
		}
	}

    public static partial class MapperHelper
    {
        public static IPAddress GetIP(string ipAddress)
        {
            IPAddress parsedAddress;
            try
            {
                parsedAddress = IPAddress.Parse(ipAddress);
            }
            catch
            {
                throw new FormatException("Invalid IP address format.");
            }

            return parsedAddress;
        }
    }
}
