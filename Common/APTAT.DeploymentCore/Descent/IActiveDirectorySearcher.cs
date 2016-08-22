using System;

namespace APTAT.DeploymentCore.Descent
{
	public interface IActiveDirectorySearcher
	{
		void Search( string path, Action<IComputer[]> foundComputersAction );
		IAsyncResult BeginSearch( string path, Action<IComputer[]> foundComputersAction, AsyncCallback callback );
		void EndSearch( IAsyncResult result );
	}
}
