using System;
using System.Collections.Generic;
using System.Diagnostics;
using APTAT.Core;
using APTAT.Core.Descent;
using APTAT.Core.Properties;
using APTAT.Core.Descent.Transport;
using APTAT.Core.Tasks;

namespace APTAT.Core.Descent.Listeners
{
	public class PackageInstallCompletedListener
	{
		public PackageInstallCompletedListener( IDescentQueryResult descentResult, ICollection<long> tasks, object taskSync )
		{
			_taskSync = taskSync;
			_tasks = tasks;
			_descentResult = descentResult;
		}

		private readonly object _taskSync;
		private readonly ICollection<long> _tasks;
		private readonly IDescentQueryResult _descentResult;

		public void ActionCompleted( object sender, ActionCompletedEventArgs args )
		{
			bool myTask = false;
			lock( _taskSync )
			{
				myTask = _tasks.Contains( args.ActionResult.Owner.Owner.Task.Key );
			}

			IActionFactory actionFactory = args.ActionResult.Action.Factory;

			if( myTask && actionFactory is ActionFactoryInstall )
			{
				ResultStatus status = args.ActionResult.Status;
				bool instSucceeded = status == ResultStatus.Succeeded;
				IComputer installedComp = args.ActionResult.Owner.Target;
				FormattedText message = args.ActionResult.ErrorMessage;
				if( String.IsNullOrEmpty( message.ToString() ) )
				{
					message = new FormattedText( ServerSideMessagesResources.DescentPackageCompleted );
				}

				LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
					"Package descent operation completed{0} computer: {1}{0} task ID: {2}{0} action status: {3}{0} error message: {4}{0}",
					Environment.NewLine, installedComp.Name, args.ActionResult.Owner.Owner.Task.Key, status, message);

				_descentResult.OnPackageInstallationEvent(
					instSucceeded,
					installedComp,
					message,
					message,
					( instSucceeded ? DescentStatus.status_success : DescentStatus.status_installer_fatal_error ).ToString(),
					message.ToString(),
					0 );
			}
		}
	}
}
