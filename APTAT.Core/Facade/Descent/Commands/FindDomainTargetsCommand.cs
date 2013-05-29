using System.Linq;
using APTAT.Core.Directory;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.Core.Facade.Descent.MessageContracts;
using System;
using Esm.Server.Facade.Descent.Commands;

namespace APTAT.Core.Facade.Descent.Commands
{
	public class FindDomainTargetsCommand : ServiceCommand<FindTargetsRequest, FindTargetsResponse>
	{
        // TODO: Probably should be moved to some Initialization part
	    private readonly IActiveDirectoryObserver _activeDirectoryObserver = new ActiveDirectoryObserver();

		public FindDomainTargetsCommand( FindTargetsRequest request )
			: base( request )
		{
		}

		public override void Execute()
		{
			//var directory = SubSystemFactory.Current.GetSubSystem<IDirectorySubSystem>();
            var directoryObserver = _activeDirectoryObserver;//directory.ActiveDirectoryObserver;
			var criteria = ( DomainSearchCriterion )Request.SearchCriterion;
			if( criteria.FromRoot )
			{
				IAdEntry root;
				try
				{
					root = directoryObserver.GetDomainRootEntry( criteria.Domain, criteria.Login, criteria.Password );
				}
				catch( DirectoryQueryException ex )
				{
					switch( ex.Error )
					{
						case DirectoryError.DefaultDomainNotFound:
                            //throw new FaultException<DefaultDirectoryDomainRootNotFoundFault>(
                            //    new DefaultDirectoryDomainRootNotFoundFault { Domain = String.Empty },
                            //    "The server is not in domain or domain could not be contacted under current credentials." );
                            throw new InvalidOperationException("The server is not in domain or domain could not be contacted under current credentials.");
						case DirectoryError.SpecifiedDomainNotFound:
                            //throw new FaultException<DirectoryDomainRootQueryFault>(
                            //    new DirectoryDomainRootQueryFault { Domain = ex.Path },
                            //    String.Format( "Domain '{0}' does not exist or could not be contacted under the specified credentials.", ex.Path ) );
                            throw new InvalidOperationException(String.Format("Domain '{0}' does not exist or could not be contacted under the specified credentials.", ex.Path));
						case DirectoryError.Unknown:
						default:
                            //throw new FaultException<DirectoryDomainRootQueryFault>(
                            //    new DirectoryDomainRootQueryFault { Domain = ex.Path },
                            //    "Unknown error accessing domain." );
                            throw new InvalidOperationException("Unknown error accessing domain.");
					}
				}

				Response.Targets.Add( new DomainTarget
				{
					TargetUid = Guid.NewGuid(),
					EntryType = DomainEntryType.Root,
					LdapPath = root.FullPath,
					Name = root.Name,
					DnsName = root.DnsName
				} );

				criteria.ParentAddress = root.FullPath;
			}

			FindComputersContainer( directoryObserver, criteria.ParentAddress, criteria );
			FindChildren( directoryObserver, criteria.ParentAddress, criteria );

			var computers = Response.Targets
				.OfType<DomainTarget>()
				.Where( target => target.EntryType == DomainEntryType.Computer );

			if( computers.Count() > 0 )
			{
                //TODO: Commented
				//TargetsHelper.FindManagedTargetsAndEsmServerHost( computers );
			}

			Response.IsCompleted = true;
		}

		private void FindComputersContainer( IActiveDirectoryObserver directoryObserver, string path, DomainSearchCriterion criterion )
		{
			try
			{
				var containers = directoryObserver.GetChildEntries( path, "container", criterion.Login, criterion.Password );
				var computersContainer = containers.FirstOrDefault( e => e.Name == "Computers" );
				if( computersContainer != null )
				{
					Response.Targets.Add( new DomainTarget
					{
						TargetUid = Guid.NewGuid(),
						LdapPath = computersContainer.FullPath,
						Name = computersContainer.Name,
						DnsName = computersContainer.DnsName,
						EntryType = DomainEntryType.OrganizationUnit,
					} );
				}
			}
			catch( DirectoryQueryException )
			{
				//throw new FaultException<DirectoryQueryFault>( new DirectoryQueryFault { ParentPath = path, Criterion = criterion }, "Error querying directory path." );
                throw new InvalidOperationException("Error querying directory path.");
			}
		}

		private void FindChildren( IActiveDirectoryObserver directoryObserver, string path, DomainSearchCriterion criterion )
		{
			IAdEntry[] children;
			try
			{
				children = directoryObserver.GetChildEntries( path, criterion.Login, criterion.Password );
			}
			catch( DirectoryQueryException )
			{
				//throw new FaultException<DirectoryQueryFault>( new DirectoryQueryFault { ParentPath = path, Criterion = criterion }, "Error querying directory path." );
                throw new InvalidOperationException("Error querying directory path.");
			}

			foreach( var child in children )
			{
				Response.Targets.Add( new DomainTarget
				{
					TargetUid = Guid.NewGuid(),
					LdapPath = child.FullPath,
					Name = child.Name,
					DnsName = child.DnsName,
					EntryType = child.Type.ToFacade(),
				} );
			}
		}

        
	}

    public static partial class MapperHelper
    {
        public static DomainEntryType ToFacade(this AdEntryType type)
        {
            switch (type)
            {
                case AdEntryType.Computer:
                    return DomainEntryType.Computer;
                case AdEntryType.OrganizationUnit:
                    return DomainEntryType.OrganizationUnit;
                case AdEntryType.AdRoot:
                    return DomainEntryType.Root;
                default:
                    throw new ArgumentException("type");
            }
        }
    }
}
