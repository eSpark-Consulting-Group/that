

namespace DDT.Application.Transitions
{
    internal class NoneTransitionStrategy : TransitionStrategyBase
    {
        public override void Initialize(TransitionPresenter transPresenter)
        {
            base.Initialize(transPresenter);
            Presenter.Transition = null;
        }
    }
}