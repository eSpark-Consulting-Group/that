using System;
using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.Core.Facade.Descent.MessageContracts;
using Esm.Server.Facade.Descent.Commands;

namespace APTAT.Core.Facade.Descent.Commands
{
	public class CommandFactory : ICommandFactory
	{
		public IServiceCommand CreateCommand( object request )
		{
			throw new NotImplementedException( "Not implemented yet." );
		}

		public IServiceCommand<TResponse> CreateCommand<TResponse>( object request ) where TResponse : new()
		{
			if( request == null )
			{
				throw new ArgumentNullException( "request" );
			}

			IServiceCommand<TResponse> command = null;

			if( request is FindTargetsRequest )
			{
				var concreteRequest = ( FindTargetsRequest )request;
				if( concreteRequest.SearchCriterion is EndpointSearchCriterion )
				{
					command = ( IServiceCommand<TResponse> )new FindEndpointTargetsCommand( concreteRequest );
				}
				else if( concreteRequest.SearchCriterion is DomainSearchCriterion )
				{
					command = ( IServiceCommand<TResponse> )new FindDomainTargetsCommand( concreteRequest );
				}
				else if( concreteRequest.SearchCriterion is WorkgroupSearchCriterion )
				{
					command = ( IServiceCommand<TResponse> )new FindWorkgroupTargetsCommand( concreteRequest );
				}
				else
				{
					throw new ArgumentException( "Unknown FindTargetsRequest.SearchCriteria instance type.", "request" );
				}
			}
			else if ( request is GetTargetsSummaryRequest )
			{
				command = ( IServiceCommand<TResponse> ) new GetTargetsSummaryCommand( ( GetTargetsSummaryRequest )request );
			}
            else if( request is DeployAgentRequest )
            {
                command = ( IServiceCommand<TResponse> )new DeployAgentCommand( ( DeployAgentRequest )request );
            }
			else if( request is GetDeploymentResultsRequest )
			{
				command = ( IServiceCommand<TResponse> )new GetDeploymentResultsCommand( ( GetDeploymentResultsRequest )request );
			}
			else if( request is GetDescentErrorMessageRequest )
			{
				command = ( IServiceCommand<TResponse> )new GetDescentErrorMessageCommand( ( GetDescentErrorMessageRequest )request );
			}
			else
				throw new ArgumentException( String.Format( "Unknown request instance type: {0}.", request.GetType() ), "request" );

			return command;
		}

		public IServiceCommand<TResponse> CreateCommand<TResponse>() where TResponse : new()
		{
			IServiceCommand<TResponse> command = null;

			#region BeginEnumerateWorkgroups

			if( typeof( TResponse ) == typeof( EnumerateWorkgroupsResponse ) )
			{
				command = ( IServiceCommand<TResponse> )new EnumerateWorkgroupsCommand();
			}

			#endregion

			#region Unknown Operation

			else
			{
				throw new ArgumentException( String.Format( "Unknown response type: {0}.", typeof( TResponse ) ), "TResponse" );
			}

			#endregion

			return command;
		}
	}
}
