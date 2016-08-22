using System;
using System.Linq;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.DeploymentCore.Descent
{
	public class ActiveDirectorySearcher : IActiveDirectorySearcher
	{
        private readonly IActiveDirectoryObserver _activeDirectoryObserver;
		private readonly string _login;
		private readonly string _password;

        public ActiveDirectorySearcher(IActiveDirectoryObserver activeDirectoryObserver, string login, string password)
        {
            _activeDirectoryObserver = activeDirectoryObserver;
            _login = login;
            _password = password;
        }

		public void Search( string path, Action<IComputer[]> foundComputersAction )
		{
			var asyncResult = BeginSearch( path, foundComputersAction );
			asyncResult.AsyncWaitHandle.WaitOne();
			EndSearch( asyncResult );
		}

		public IAsyncResult BeginSearch( string path, Action<IComputer[]> foundComputersAction, AsyncCallback callback = null )
		{
			Action action = () =>
			{
				FindComputers( path, foundComputersAction );
			};

			return action.BeginInvoke( callback, action );
		}

		private void FindComputers( string path, Action<IComputer[]> foundComputersAction )
		{
			IComputer[] computers = new IComputer[0];
			var entries = _activeDirectoryObserver.GetChildEntries( path, _login, _password );

			var grouppedEntries = from entry in entries
								  group entry by entry.Type into entryGrp
								  select entryGrp;

		    var enumerable = grouppedEntries as IGrouping<AdEntryType, IAdEntry>[] ?? grouppedEntries.ToArray();
		    var computersGroup = enumerable.FirstOrDefault( grp => grp.Key == AdEntryType.Computer );
			if( computersGroup != null )
			{
                computers = ( from entry in computersGroup
							  select new DomainTarget()
							  {
                                  SystemKind = SystemKindParser.TryParse(entry.OsName, entry.OsVersion),
                                  Name = entry.Name,
								  DnsName = entry.DnsName,
                                  LdapPath = entry.FullPath,
                                  EntryType = DomainEntryType.Computer,
							  } ).ToArray();
			}

			if( computers.Length > 0 && foundComputersAction != null )
			{
				foundComputersAction( computers );
			}

			var unitsGroup = enumerable.FirstOrDefault( grp => grp.Key == AdEntryType.OrganizationUnit );
			if( unitsGroup != null )
			{
				foreach( var unit in unitsGroup )
				{
					FindComputers( unit.FullPath, foundComputersAction );
				}
			}
		}

		public void EndSearch( IAsyncResult result )
		{
			Action action = ( Action )result.AsyncState;
			action.EndInvoke( result );
		}
	}
}
