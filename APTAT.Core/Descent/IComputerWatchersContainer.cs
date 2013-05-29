using System;
using APTAT.Core.Descent;

namespace CRM.SrvCore.Descent
{
	public interface IComputerWatchersContainer
	{
		void AddWatcher( Guid computerUid, IDescentWatcher<ComputerDescentWatcherParams> watcher );
		bool TryRemoveWatcher( Guid computerUid, out IDescentWatcher<ComputerDescentWatcherParams> watcher );
		IDescentWatcher<ComputerDescentWatcherParams>[] Watchers { get; }
	}
}
