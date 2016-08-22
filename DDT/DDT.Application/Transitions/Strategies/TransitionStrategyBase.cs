
using System.Windows;

namespace DDT.Application.Transitions
{
    public interface ITransitionStrategy
    {
        bool ApplyTransition(FrameworkElement target, Direction? direction);
        void Initialize(TransitionPresenter transPresenter);

        FrameworkElement CurrentTarget { get; set; }
        TransitionPresenter Presenter { get; }
    }

    public abstract class TransitionStrategyBase : ITransitionStrategy
    {
        protected TransitionStrategyBase()
        {
        }

        public virtual bool ApplyTransition(FrameworkElement target, Direction? direction)
        {
            if (target != CurrentTarget)
            {
                if (Presenter.ApplyTransition(CurrentTarget, target))
                {
                    CurrentTarget = target;
                    return true;
                }

                return false;
            }

            return false;
        }

        public virtual void Initialize(TransitionPresenter transPresenter)
        {
            Presenter = transPresenter;
        }

        public FrameworkElement CurrentTarget { get; set; }

        public TransitionPresenter Presenter { get; protected set; }
    }
}
