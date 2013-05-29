using System;
using System.Collections.Generic;
using APTAT.Core.Tasks;
using APTAT.Core.Messages;

namespace APTAT.Core.Descent.Listeners
{
	public class DescentCompletedListener
	{
		public DescentCompletedListener( ComputerWatchersContainer watchersContainer, ITasksSubSystem tasks, IMessagingSubSystem messaging,
			IDescentQueryResult result, Action<IComputer, bool> agentDescentCompletedDelegate, EventHandler<ActionDeferredEventArgs> actionDeferredDelegate,
			EventHandler<ProductRegisteredEventArgs> computerConnectedDelegate,
			IEnumerable<EventHandler<ActionCompletedEventArgs>> actionCompletedDelegates,
			EventHandler<ActionStartedEventArgs> actionStartedDelegate )
		{
			_watchersContainer = watchersContainer;
			_tasks = tasks;
			_messaging = messaging;
			_result = result;
			_agentDescentCompletedDelegate = agentDescentCompletedDelegate;
			_actionDeferredDelegate = actionDeferredDelegate;
			_computerConnectedDelegate = computerConnectedDelegate;
			_actionStartedDelegate = actionStartedDelegate;
			_actionCompletedDelegates = actionCompletedDelegates;
		}

		private readonly ComputerWatchersContainer _watchersContainer;
		private readonly ITasksSubSystem _tasks;
		private readonly IDescentQueryResult _result;
		private readonly IMessagingSubSystem _messaging;

		private readonly Action<IComputer, bool> _agentDescentCompletedDelegate;
		private readonly EventHandler<ActionDeferredEventArgs> _actionDeferredDelegate;
		private readonly EventHandler<ProductRegisteredEventArgs> _computerConnectedDelegate;
		private readonly IEnumerable<EventHandler<ActionCompletedEventArgs>> _actionCompletedDelegates;
		private readonly EventHandler<ActionStartedEventArgs> _actionStartedDelegate;
		private Action _installPackageDescentCompletedDelegate;

		public void SetDescentCompletedDelegate( Action descentCompletedDelegate )
		{
			_installPackageDescentCompletedDelegate = descentCompletedDelegate;
		}

		public void DecsentCompleted()
		{
			foreach( var watcher in _watchersContainer.Watchers )
			{
				watcher.Dispose();
			}

			foreach (var actionCompletedDelegate in _actionCompletedDelegates)
			{
				_tasks.ActionCompleted -= actionCompletedDelegate;
			}

			_tasks.ActionChanged -= _actionStartedDelegate;

            // Commented out: does not work good for CIS update (shows 100% immediately before 1st reboot)
            //			_tasks.ActionDeferred -= _actionDeferredDelegate;
			_messaging.ProductRegistered -= _computerConnectedDelegate;
			_result.AgentDescentCompleted -= _agentDescentCompletedDelegate;
			_result.DescentCompleted -= _installPackageDescentCompletedDelegate;
		}
	}
}
