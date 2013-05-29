namespace APTAT.Core.Descent
{
	public interface IDescentWatcherFactory
	{
		IDescentWatcher<TParams> CreateWatcher<TParams>( TParams watcherParams ) where TParams : DescentWatcherParams;
	}
}
