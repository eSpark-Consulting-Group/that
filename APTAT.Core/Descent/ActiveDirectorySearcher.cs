using System;
using System.Linq;

namespace APTAT.Core.Descent
{
	public class ActiveDirectorySearcher : IActiveDirectorySearcher
	{
        private readonly IActiveDirectoryObserver _activeDirectoryObserver;
		private readonly string _login;
		private readonly string _password;

		public ActiveDirectorySearcher( IDirectorySubSystem directory )
			: this( directory, null, null )
		{
		}

		public ActiveDirectorySearcher( IDirectorySubSystem directory, string login, string password )
		{
			_login = login;
			_password = password;
            _activeDirectoryObserver = directory.ActiveDirectoryObserver;
		}

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

			var computersGroup = grouppedEntries.FirstOrDefault( grp => grp.Key == AdEntryType.Computer );
			if( computersGroup != null )
			{
				computers = ( from entry in computersGroup
							  select new DirectoryComputer
							  {
								  Name = entry.Name,
								  DnsName = entry.DnsName,
                                  LdapPath = entry.FullPath
							  } ).ToArray();
			}

			if( computers.Length > 0 && foundComputersAction != null )
			{
				foundComputersAction( computers );
			}

			var unitsGroup = grouppedEntries.FirstOrDefault( grp => grp.Key == AdEntryType.OrganizationUnit );
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
