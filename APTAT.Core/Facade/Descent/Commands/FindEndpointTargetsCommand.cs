using System;
using System.Collections.Generic;
using System.Net;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using APTAT.Core.Facade.Descent.MessageContracts;
using APTAT.Core.Transport;
using Esm.Server.Facade.Descent.Commands;

namespace APTAT.Core.Facade.Descent.Commands
{
    public enum AddressInvalidityReason
    {
        IPAddressFormat,

        NetworkMaskValue,

        DomainNameFormat,

        IPAddressesRangeTooLarge,
    }

    public class AddressReason
    {
        public int AddressKey { get; set; }
        public AddressInvalidityReason Reason { get; set; }
    }

    public class AddressReasonCollection : List<AddressReason>
    {
    }

    

	public class FindEndpointTargetsCommand : ServiceCommand<FindTargetsRequest, FindTargetsResponse>, IEqualityComparer<DeploymentTarget>
	{
		public FindEndpointTargetsCommand( FindTargetsRequest request )
			: base( request )
		{
			IPSatretegySelectionProvider = new IPSatretegySelectionProvider();
		}

		public IIPSatretegySelectionProvider IPSatretegySelectionProvider
		{
			get;
			set;
		}

		public override void Execute()
		{
			var criteria = ( EndpointSearchCriterion )Request.SearchCriterion;
			var endpointTargets = new HashSet<EndpointTarget>( this );
			var faultedAddresses = new AddressReasonCollection();
			foreach( var address in criteria.EndpointAddresses )
			{
				if( address is SingleIPAddress )
				{
					try
					{
						var ip = IPAddress.Parse( address.ToString() );
						endpointTargets.Add( new EndpointTarget { TargetUid = Guid.NewGuid(), Name = ip.ToString(), IPAddress = ip.ToString() } );
					}
					catch( FormatException )
					{
						faultedAddresses.Add( new AddressReason { AddressKey = address.Key, Reason = AddressInvalidityReason.IPAddressFormat } );
					}
				}
				else if( address is DnsNameAddress )
				{
					endpointTargets.Add( new EndpointTarget { TargetUid = Guid.NewGuid(), Name = address.ToString(), DnsName = address.ToString() } );
				}
				else
				{
					IIPSelectionStrategy strategy = null;
					try
					{
						strategy = IPSatretegySelectionProvider.GetStrategy( address );
					}
					catch( InvalidAddressException ex )
					{
						faultedAddresses.Add( new AddressReason { AddressKey = address.Key, Reason = ex.Reason.ToFacade() } );
					}
					catch( FormatException )
					{
						faultedAddresses.Add( new AddressReason { AddressKey = address.Key, Reason = AddressInvalidityReason.IPAddressFormat } );
					}

					if( faultedAddresses.Count > 0 )
						continue;

					IEnumerable<IPAddress> ips;
					try
					{
						ips = strategy.GetIPs();
					}
					catch( InvalidOperationException )
					{
						ips = new IPAddress[0];
						Response.Targets.Add( new EndpointTarget { TargetUid = Guid.NewGuid(), Name = address.ToString() } );
					}

					foreach( var ip in ips )
					{
						endpointTargets.Add( new EndpointTarget { TargetUid = Guid.NewGuid(), Name = ip.ToString(), IPAddress = ip.ToString() } );
					}
				}
			}

			if( faultedAddresses.Count > 0 )
			{
                //throw new FaultException<InvalidEndpointAddressFault>(
                //    new InvalidEndpointAddressFault { Reasons = faultedAddresses },
                //    "One on more endpoint addresses are invalid." );
                throw new InvalidOperationException("One on more endpoint addresses are invalid.");
			}

			Response.Targets.AddRange( endpointTargets );
			Response.IsCompleted = true;
		}

		public bool Equals(DeploymentTarget x, DeploymentTarget y)
		{
			return x.IPAddress == y.IPAddress || (String.IsNullOrEmpty(x.IPAddress) && String.IsNullOrEmpty(y.IPAddress) && x.Name == y.Name);
		}

		public int GetHashCode(DeploymentTarget obj)
		{
			return !String.IsNullOrEmpty(obj.IPAddress) ? obj.IPAddress.GetHashCode() : obj.Name.GetHashCode();
		}
	}

    public static partial class MapperHelper
    {
        public static AddressInvalidityReason ToFacade(this InvalidityReason reason)
        {
            switch (reason)
            {
                case InvalidityReason.InvalidHostName:
                    return AddressInvalidityReason.DomainNameFormat;
                case InvalidityReason.InvalidSubnetMask:
                    return AddressInvalidityReason.NetworkMaskValue;
                case InvalidityReason.TooManyAddressesInRange:
                    return AddressInvalidityReason.IPAddressesRangeTooLarge;
                default:
                    throw new ArgumentException("reason");
            }
        }
    }
}
