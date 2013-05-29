using System;

namespace APTAT.Core.Descent
{
	public interface IDescentWatcher<TParams> : IDisposable where TParams: DescentWatcherParams
	{
		TParams Params { get; set; }
		void Watch();
	}
}
