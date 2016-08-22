
using System.Windows;

namespace DDT.Application.Transitions
{
    internal class CubeTransitionStrategy : TransitionStrategyBase
    {
        public override bool ApplyTransition(FrameworkElement target, Direction? direction)
        {
            if (target != CurrentTarget)
            {
                if (CurrentTarget == null)
                    CurrentTarget = target;

                CubeTransition transition = Presenter.Transition as CubeTransition;

                if (direction.HasValue)
                {
                    transition.Rotation = direction.Value;
                    return base.ApplyTransition(target, direction);
                }

                var currentLevel = int.Parse(CurrentTarget.Tag.ToString());
                int tag = int.Parse(target.Tag.ToString());

                if (tag > currentLevel)
                {
                    transition.Rotation = Direction.BottomToTop;
                }
                else if (tag == currentLevel)
                {
                    transition.Rotation = Direction.LeftToRight;
                }
                else if (tag < currentLevel)
                {
                    transition.Rotation = Direction.TopToBottom;
                }

                return base.ApplyTransition(target, direction);
            }

            return false;
        }

        public override void Initialize(TransitionPresenter transPresenter)
        {
            base.Initialize(transPresenter);
            Presenter.Transition = new CubeTransition();
        }
    }
}