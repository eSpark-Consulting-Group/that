using System;
using System.Collections.Concurrent;
using System.Linq;
using CRM.SrvCore.Descent;

namespace APTAT.Core.Descent
{
	public class ComputerWatchersContainer : IComputerWatchersContainer
	{
		private readonly ConcurrentDictionary<Guid, IDescentWatcher<ComputerDescentWatcherParams>> _watchers;

		public ComputerWatchersContainer(ConcurrentDictionary<Guid, IDescentWatcher<ComputerDescentWatcherParams>> watchers)
		{
			if(watchers==null)
			{
				throw new ArgumentNullException( "watchers" );
			}

			_watchers = watchers;
		}

		public void AddWatcher( Guid computerUid, IDescentWatcher<ComputerDescentWatcherParams> watcher )
		{
			_watchers.AddOrUpdate( computerUid, watcher, ( uid, w ) => watcher );
		}

		public bool TryRemoveWatcher( Guid computerUid, out IDescentWatcher<ComputerDescentWatcherParams> watcher )
		{
			return _watchers.TryRemove( computerUid, out watcher );
		}

		public IDescentWatcher<ComputerDescentWatcherParams>[] Watchers
		{
			get
			{
				return _watchers.Values.ToArray();
			}
		}
	}
}
