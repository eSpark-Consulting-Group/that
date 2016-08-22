using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.DeploymentCore.Facade.Descent.MessageContracts;

namespace APTAT.DeploymentCore.Facade.Descent.Commands
{
	public class FindWorkgroupTargetsCommand : ServiceCommand<FindTargetsRequest, FindTargetsResponse>
	{
	    private readonly INetworkBrowser _networkBrowser;
		public FindWorkgroupTargetsCommand( INetworkBrowser networkBrowser, FindTargetsRequest request )
			: base( request )
		{
		    _networkBrowser = networkBrowser;
		}
        

		public override void Execute()
		{
			var criteria = ( WorkgroupSearchCriterion )Request.SearchCriterion;
			List<string> computers = new List<string>();
			var result = _networkBrowser.GetNetworkComputers( ServerType.All, criteria.WorkgroupName, computers );
			if( result == NetServerEnumResults.ERROR_INVALID_PARAMETER )
			{
				//throw new FaultException<WorkgroupInvalidFault>( new WorkgroupInvalidFault { WorkgroupName = criteria.WorkgroupName }, "Workgroup name is invalid." );
                throw new InvalidOperationException("Workgroup name is invalid.");
			}
			else if( result == NetServerEnumResults.ERROR_NO_BROWSER_SERVERS_FOUND )
			{
				//throw new FaultException<WorkgroupNotFoundFault>( new WorkgroupNotFoundFault { WorkgroupName = criteria.WorkgroupName }, "Workgroup not found." );
                throw new InvalidOperationException("Workgroup not found.");
			}
			else if( result == NetServerEnumResults.ERROR_MORE_DATA )
			{
				//throw new FaultException<WorkgroupQueryFault>( new WorkgroupQueryFault { WorkgroupName = criteria.WorkgroupName }, "Too many items are in the workgroup." );
                throw new InvalidOperationException("Too many items are in the workgroup.");
			}
            else if (result == NetServerEnumResults.UNKNOWN_ERROR)
            {
                throw new InvalidOperationException("Unknown error occured.");
            }

			Response.Targets.AddRange( from name in computers select new WorkgroupTarget { Uid = Guid.NewGuid(), Name = name, SmbName = name } );
			Response.IsCompleted = true;
		}
	}
}
