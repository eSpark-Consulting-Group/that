using System;
using System.Collections.Generic;
using System.Diagnostics;
using APTAT.Core.Properties;
using CRM.SrvCore.Descent;
using APTAT.Core.Descent.Transport;
using APTAT.Core.Tasks;

namespace APTAT.Core.Descent.Listeners
{
	public class ActionDeferredListener
	{
		public ActionDeferredListener( IDescentQueryResult descentResult, InstallPackageInfo packageInfo, object taskSync, ICollection<long> tasksKeys )
		{
			_descentResult = descentResult;
			_package = packageInfo;
			_taskSync = taskSync;
			_tasksKeys = tasksKeys;
		}

		private readonly object _taskSync;
		private readonly IDescentQueryResult _descentResult;
		private readonly InstallPackageInfo _package;
		private readonly ICollection<long> _tasksKeys;

		public void ActionDeferred( object sender, ActionDeferredEventArgs args )
		{
			lock( _taskSync )
			{
				if( !_tasksKeys.Contains( args.ActionResult.Owner.Owner.Task.Key ) )
					return;
			}

			if( !( args.ActionResult.Action.Factory is ActionFactoryInstall ) )
				return;

			IComputer deferredComputer = args.ActionResult.Owner.Owner.Task.Target.GetTargets()[0];

			LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
				"Action was set as deferred. Package descent completed before reboot.{0} computer: {1}{0} computer UID: {2}{0} task ID: {3}{0}",
				Environment.NewLine, deferredComputer.Name, deferredComputer.Uid, args.ActionResult.Owner.Owner.Task.Key );

			if( _package.SuppressReboot )
			{
				FormattedText statusMessage = new FormattedText( ServerSideMessagesResources.DescentPackageCompletedRebootRequired );
				_descentResult.OnPackageInstallationEvent( true, deferredComputer, statusMessage, statusMessage, DescentStatus.status_success.ToString(), String.Empty, 0 );
			}
			else
			{
				FormattedText statusMessage = new FormattedText( ServerSideMessagesResources.DescentPackageCompletedRebootingInProgress );
				_descentResult.OnPackageInstallationEvent( true, deferredComputer, statusMessage, statusMessage, DescentStatus.status_success.ToString(), String.Empty, 0 );
			}
		}
	}
}
