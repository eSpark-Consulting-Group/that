using System;
using System.Diagnostics;
using System.Threading;
using APTAT.Core.Properties;
using APTAT.Core;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Transport;

namespace APTAT.Core.Descent
{
	public class AgentDescentWatcher : BaseDescentWatcher<AgentDescentWatcherParams>
	{
		public AgentDescentWatcher( AgentDescentWatcherParams watcherParams ) : base( watcherParams )
		{
		}

		public override TimeSpan Timeout
		{
			get { return Settings.Default.AgentDescentingTimeout; }
		}

		protected override bool Terminate()
		{
			LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent, "Descent timeout timer elapsed");

			if( !Params.DescentCallback.IsCompleted )
			{
				try
				{
					Params.DescentThread.Abort( DescentMod.TimeoutAbortString );
					LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent,
						"Queued thread with descenting operation has been aborted by timeout{0} computer: {1}{0} timeout: {2}{0} thread ID: {3}{0}",
						Environment.NewLine, Params.Computer, Settings.Default.AgentDescentingTimeout, Params.DescentThread.ManagedThreadId);

					Params.DescentCallback.OnDescentCallback( DescentStatus.status_agent_timeout, 0,
						new FormattedText(ServerSideMessagesResources.DescentTimeoutSystemMessage, Timeout.ToString() ).ToString() );
				}
				catch( ThreadStateException ex )
				{
					LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent, "Thread abort exception occurred: {0}", ex);
				}
			}
			return true;
		}
	}
}
