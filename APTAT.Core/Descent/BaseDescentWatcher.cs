using System;
using System.Threading;
using System.Diagnostics;
using CRM.SrvCore.Descent;

namespace APTAT.Core.Descent
{
	public abstract class BaseDescentWatcher<TParams> : IDescentWatcher<TParams> where TParams : DescentWatcherParams
	{
		private Timer _timer;

		public BaseDescentWatcher( TParams watcherParams )
		{
			Params = watcherParams;
		}

		public TParams Params
		{
			get;
			set;
		}

		public abstract TimeSpan Timeout
		{
			get;
		}

		public void Watch()
		{
			//Debug.WriteLine( String.Format( "Descent watcher started{0} watcher type: {1}{0} timeout: {2}{0}",
			//	Environment.NewLine, GetType(), Timeout ), "DSC" );

			_timer = null;
			_timer = new Timer( timerState =>
			{
				LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
					"Descent watcher timed out{0} watcher type: {1}{0} timeout: {2}{0}",
					Environment.NewLine, GetType(), Timeout );

				if (Terminate())
				{
					_timer.Dispose();
				}
				else
				{
					_timer.Change(Timeout, TimeSpan.Zero);
				}
			}, 
			null, Timeout, TimeSpan.Zero );
		}

		protected abstract bool Terminate();

		public void Dispose()
		{
			if( _timer != null )
			{
				//Debug.WriteLine( String.Format( "Descent watcher disposed{0} watcher type: {1}{0}",
				//	Environment.NewLine, GetType() ), "DSC" );

				_timer.Dispose();
			}
		}
	}
}
