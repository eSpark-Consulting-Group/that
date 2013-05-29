using APTAT.Core.Descent;
using CRM.SrvCore.Descent;

namespace APTAT.Core.Descent
{
	public class DescentWatcherFactory : IDescentWatcherFactory
	{
		public IDescentWatcher<TParams> CreateWatcher<TParams>( TParams watcherParams ) where TParams : DescentWatcherParams
		{
			if( watcherParams is ComputerDescentWatcherParams )
			{
				var concreteParams = watcherParams as ComputerDescentWatcherParams;
				return ( IDescentWatcher<TParams> )new ComputerDescentWatcher( concreteParams );
			}
			else if( watcherParams is AgentDescentWatcherParams )
			{
				var concreteParams = watcherParams as AgentDescentWatcherParams;
				return ( IDescentWatcher<TParams> )new AgentDescentWatcher( concreteParams );
			}

			return null;
		}
	}
}
