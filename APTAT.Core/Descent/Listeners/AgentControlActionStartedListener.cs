using APTAT.Core.Tasks;

namespace APTAT.Core.Descent.Listeners
{
	public class AgentControlActionStartedListener
	{
        //private readonly ComputerConnectedListener _computerConnectedListener;
        //private readonly AgentDescentCompletedListener _agentDescentCompletedListener;
        //private readonly IDescentQueryResult _descentResult;

        //public AgentControlActionStartedListener(
        //    IDescentQueryResult descentResult,
        //    ComputerConnectedListener computerConnectedListener,
        //    AgentDescentCompletedListener agentDescentCompletedListener )
        //{
        //    _descentResult = descentResult;
        //    _computerConnectedListener = computerConnectedListener;
        //    _agentDescentCompletedListener = agentDescentCompletedListener;
        //}

        //public void ActionChanged( object sender, ActionStartedEventArgs args )
        //{
        //    if( ActionResultAccepted( args.ActionResult ) )
        //    {
        //        IComputer computer = args.ActionResult.Owner.Target;
        //        _computerConnectedListener.AddActionControlledAgentComputer( computer );
        //        _agentDescentCompletedListener.AddActionControlledAgentComputer( computer );
        //    }
        //}

        //protected bool ActionResultAccepted( IActionResult actionResult )
        //{
        //    if( actionResult.Action.Factory is ActionFactoryAgentControl )
        //    {
        //        ActionFactoryAgentControl.DataInput actionInput = ( ActionFactoryAgentControl.DataInput )actionResult.Action.Input;
        //        if( actionInput.AgentControlAction == ActionFactoryAgentControl.DataInput.ControlAction.Update )
        //        {
        //            IComputer target = actionResult.Owner.Target;
        //            ComputerDescentResult computerDescentResult;
        //            if( _descentResult.ContainsComputer( target.Uid, out computerDescentResult ) )
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}
	}
}