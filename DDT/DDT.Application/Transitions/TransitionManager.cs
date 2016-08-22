
using System.Collections.ObjectModel;

namespace DDT.Application.Transitions
{
    public static class TransitionManager
    {
        private static Collection<ITransitionStrategy> _availableStrategies = new Collection<ITransitionStrategy>();
        private static ITransitionStrategy _currentStrategy;
        private static TransitionPresenter _presenter;

        static TransitionManager()
        {
            _availableStrategies.Add(new NoneTransitionStrategy());
            _availableStrategies.Add(new CubeTransitionStrategy());

            CurrentStrategy = _availableStrategies[1];
        }

        private static void Initialize()
        {
            if ((Presenter != null) && (CurrentStrategy != null))
            {
                CurrentStrategy.Initialize(Presenter);
            }
        }

        public static Collection<ITransitionStrategy> AvailableStrategies
        {
            get
            {
                return _availableStrategies;
            }
        }

        public static ITransitionStrategy CurrentStrategy
        {
            get
            {
                return _currentStrategy;
            }
            set
            {
                if ((value != null) && (_currentStrategy != null))
                {
                    value.CurrentTarget = _currentStrategy.CurrentTarget;
                }
                _currentStrategy = value;
                Initialize();
            }
        }

        public static TransitionPresenter Presenter
        {
            get
            {
                return _presenter;
            }
            set
            {
                _presenter = value;
                Initialize();
            }
        }
    }
}