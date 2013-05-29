using System;
using System.Collections.Generic;
using CRM.SrvCore.Descent;
using APTAT.Core.Tasks;

namespace APTAT.Core.Descent.Listeners
{
	public class AgentDescentCompletedListener
	{
        //public AgentDescentCompletedListener( IDescentQueryResult descentResult, IComputerWatchersContainer watchersContainer, IDescentWatcherFactory descentWatcherFactory, IChangeTrackingSubSystem tracker, ComputerConnectedListener computerConnectedListener )
        //{
        //    _descentResult = descentResult;
        //    _watchers = watchersContainer;
        //    _descentWatcherFactory = descentWatcherFactory;
        //    _tracker = tracker;
        //    _computerConnectedListener = computerConnectedListener;
        //}

        //private readonly IComputerWatchersContainer _watchers;
        //private readonly IDescentQueryResult _descentResult;
        //private readonly IDescentWatcherFactory _descentWatcherFactory;
        //private readonly IChangeTrackingSubSystem _tracker;
        //private readonly HashSet<Guid> _actionControlledAgentsComputers = new HashSet<Guid>();
        //private readonly ComputerConnectedListener _computerConnectedListener;

        //public void AgentDescentCompleted( IComputer computer, bool success )
        //{
        //    if( success )
        //    {
        //        var watcher = _descentWatcherFactory.CreateWatcher( new ComputerDescentWatcherParams { Computer = computer, DescentResult = this._descentResult } );
        //        watcher.Watch();
        //        _watchers.AddWatcher( computer.Uid, watcher );
        //        lock( _actionControlledAgentsComputers )
        //        {
        //            if( _actionControlledAgentsComputers.Contains( computer.Uid ) )
        //            {
        //                _computerConnectedListener.ProductRegistered( this, new ProductRegisteredEventArgs( computer, PluginGuids.CisGuid ) );
        //                return;
        //            }
        //        }

        //        NotifyProgressChanged(computer.Uid, ServerSideMessagesResources.DescentAgentWaitingConnectionMessage, 0);
        //    }
        //}

        //private void NotifyProgressChanged( Guid computerUid, ResourceText resourceMessage, int percentage )
        //{
        //    FormattedText formattedText = new FormattedText( resourceMessage );
        //    _tracker.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed, new DescentProgressItem(computerUid, _descentResult.OperationUid, formattedText, percentage)), false);
        //}

        //public void AddActionControlledAgentComputer( IComputer computer )
        //{
        //    lock( _actionControlledAgentsComputers )
        //    {
        //        _actionControlledAgentsComputers.Add( computer.Uid );
        //    }
        //}
	}
}
