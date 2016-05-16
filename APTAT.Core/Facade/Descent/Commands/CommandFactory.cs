using System;
using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.Core.Facade.Descent.MessageContracts;

namespace APTAT.Core.Facade.Descent.Commands
{
	public class CommandFactory : ICommandFactory
	{
	    private static CommandFactory _instance;
        private CommandFactory ()
        { }

	    public static CommandFactory Instance
	    {
	        get { return _instance ?? (_instance = new CommandFactory()); }
	    }

		public IServiceCommand<TResponse> CreateCommand<TResponse>( object request ) where TResponse : new()
		{
			if( request == null )
			{
				throw new ArgumentNullException( "request" );
			}

			IServiceCommand<TResponse> command;

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
			else
				throw new ArgumentException( String.Format( "Unknown request instance type: {0}.", request.GetType() ), "request" );

			return command;
		}

		
	}
}
