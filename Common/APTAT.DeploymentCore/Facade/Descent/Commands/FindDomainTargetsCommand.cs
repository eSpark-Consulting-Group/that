using System;
using System.Linq;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.DeploymentCore.Facade.Descent.MessageContracts;

namespace APTAT.DeploymentCore.Facade.Descent.Commands
{
	public class FindDomainTargetsCommand : ServiceCommand<FindTargetsRequest, FindTargetsResponse>
	{
	    private readonly IActiveDirectoryObserver _activeDirectoryObserver;
        public FindDomainTargetsCommand(IActiveDirectoryObserver activeDirectoryObserver, FindTargetsRequest request )
			: base( request )
        {
            _activeDirectoryObserver = activeDirectoryObserver;

        }

		public override void Execute()
		{
			var criteria = ( DomainSearchCriterion )Request.SearchCriterion;
			if( criteria.FromRoot )
			{
				IAdEntry root;
				try
				{
					root = _activeDirectoryObserver.GetDomainRootEntry( criteria.Domain, criteria.Login, criteria.Password );
				}
				catch( DirectoryQueryException ex )
				{
					switch( ex.Error )
					{
						case DirectoryError.DefaultDomainNotFound:
                            //throw new FaultException<DefaultDirectoryDomainRootNotFoundFault>(
                            //    new DefaultDirectoryDomainRootNotFoundFault { Domain = String.Empty },
                            //    "The server is not in domain or domain could not be contacted under current credentials." );
                            throw;
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
					Uid = Guid.NewGuid(),
					EntryType = DomainEntryType.Root,
                    DomainName = root.DomainName,
					LdapPath = root.FullPath,
					Name = root.Name,
					DnsName = root.DnsName
				} );

				criteria.ParentAddress = root.FullPath;
			}
			else
			{
                FindComputersContainer(_activeDirectoryObserver, criteria.ParentAddress, criteria);
                FindChildren(_activeDirectoryObserver, criteria.ParentAddress, criteria);

                var computers = Response.Targets
                    .OfType<DomainTarget>()
                    .Where(target => target.EntryType == DomainEntryType.Computer);

                if (computers.Count() > 0)
                {
                    //TODO: Commented
                    //TargetsHelper.FindManagedTargetsAndEsmServerHost( computers );
                }
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
						Uid = Guid.NewGuid(),
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
			    SystemKind? systemKind = null;
                if ( child.Type == AdEntryType.Computer) 
                    systemKind = SystemKindParser.TryParse(child.OsName, child.OsVersion);
                Response.Targets.Add( new DomainTarget
				{
                    Uid = Guid.NewGuid(),
					LdapPath = child.FullPath,
					Name = child.Name,
					DnsName = child.DnsName,
					EntryType = child.Type.ToFacade(),
                    SystemKind = systemKind,
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
