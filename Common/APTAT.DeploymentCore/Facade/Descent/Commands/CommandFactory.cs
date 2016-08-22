using System;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.DeploymentCore.Facade.Descent.MessageContracts;

namespace APTAT.DeploymentCore.Facade.Descent.Commands
{
	public class CommandFactory : ICommandFactory
	{
	    private readonly IIPSatretegySelectionProvider _ipSatretegySelectionProvider;
	    private readonly INetworkObserver _networkObserver;
	    private readonly INetworkBrowser _networkBrowser;
	    private readonly IActiveDirectoryObserver _activeDirectoryObserver;
	    private readonly IWorkgroupObserver _workgroupObserver;
        public CommandFactory (IIPSatretegySelectionProvider ipSatretegySelectionProvider, INetworkObserver networkObserver, 
            INetworkBrowser networkBrowser, IActiveDirectoryObserver activeDirectoryObserver, IWorkgroupObserver workgroupObserver)
        {
            _ipSatretegySelectionProvider = ipSatretegySelectionProvider;
            _networkObserver = networkObserver;
            _networkBrowser = networkBrowser;
            _activeDirectoryObserver = activeDirectoryObserver;
            _workgroupObserver = workgroupObserver;
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
					command = ( IServiceCommand<TResponse> )new FindEndpointTargetsCommand( _ipSatretegySelectionProvider,
                        concreteRequest );
				}
				else if( concreteRequest.SearchCriterion is DomainSearchCriterion )
				{
					command = ( IServiceCommand<TResponse> )new FindDomainTargetsCommand(_activeDirectoryObserver, concreteRequest );
				}
				else if( concreteRequest.SearchCriterion is WorkgroupSearchCriterion )
				{
					command = ( IServiceCommand<TResponse> )new FindWorkgroupTargetsCommand(_networkBrowser, concreteRequest );
				}
				else
				{
					throw new ArgumentException( "Unknown FindTargetsRequest.SearchCriteria instance type.", "request" );
				}
			}
			else if ( request is GetTargetsSummaryRequest )
			{
				command = ( IServiceCommand<TResponse> ) new GetTargetsSummaryCommand(_activeDirectoryObserver,_workgroupObserver,
                    _networkObserver, ( GetTargetsSummaryRequest )request );
			}
			else
				throw new ArgumentException( String.Format( "Unknown request instance type: {0}.", request.GetType() ), "request" );

			return command;
		}
        public IServiceCommand<TResponse> CreateCommand<TResponse>() where TResponse : new()
        {
            IServiceCommand<TResponse> command = null;

            #region BeginEnumerateWorkgroups

            if (typeof(TResponse) == typeof(EnumerateWorkgroupsResponse))
            {
                command = (IServiceCommand<TResponse>)new EnumerateWorkgroupsCommand(_networkBrowser);
            }

            #endregion

            #region Unknown Operation

            else
            {
                throw new ArgumentException(String.Format("Unknown response type: {0}.", typeof(TResponse)), "TResponse");
            }

            #endregion

            return command;
        }

    }
}
