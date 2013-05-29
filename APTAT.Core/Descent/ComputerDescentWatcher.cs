using System;
using System.Diagnostics;
using APTAT.Core.Properties;
using APTAT.Core;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Transport;

namespace APTAT.Core.Descent
{
	public class ComputerDescentWatcher : BaseDescentWatcher<ComputerDescentWatcherParams>
	{
		public ComputerDescentWatcher( ComputerDescentWatcherParams watcherParams ) : base( watcherParams )
		{
		}

		public override TimeSpan Timeout
		{
			get { return Settings.Default.PackageDescentingTimeout; }
		}

		protected override bool Terminate()
		{
			bool terminate;
			ComputerDescentResult result;
			if (Params.DescentResult.ContainsComputer(Params.Computer.Uid, out result))
			{
				terminate = result.CurrentStage != DescentStage.OutdatedCisUninstall && result.CurrentStage != DescentStage.IncompatibleUninstall;
			}
			else
			{
				LogHelper.Trace(TraceEventType.Error, TraceCategory.Descent,
					"Computer descent result not found{0} computer name: {1}{0} computer UID: {2}{0} operation UID: {3}{0}",
					Environment.NewLine, Params.Computer.Name, Params.Computer.Uid, Params.DescentResult.OperationUid);

				return true;
			}
			if (terminate)
			{
				LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent,
					"Computer connection for package installation timed out{0} computer name: {1}{0} computer UID: {2}{0} operation UID: {3}{0} timeout: {4}{0}",
					Environment.NewLine, Params.Computer.Name, Params.Computer.Uid, Params.DescentResult.OperationUid, Timeout );

				FormattedText statusMessage = new FormattedText( ServerSideMessagesResources.PackageDescentTimeoutMessage );
				Params.DescentResult.OnPackageInstallationEvent(
					false,
					Params.Computer,
					statusMessage, 
					statusMessage,
					DescentStatus.status_package_timeout.ToString(),
					new FormattedText( ServerSideMessagesResources.DescentTimeoutSystemMessage, Timeout.ToString() ).ToString(),
					0 );

				return true;
			}
		    
            LogHelper.Trace(TraceEventType.Information, TraceCategory.Descent,
		                    "Timeout suppressed - waiting computer uninstall tasks to complete{0} computer name: {1}{0} computer UID: {2}{0} operation UID: {3}{0}",
		                    Environment.NewLine, Params.Computer.Name, Params.Computer.Uid, Params.DescentResult.OperationUid);

		    // Prevent descent operation from timeout if there are uninstall tasks pending
		    return false;
		}
	}
}
