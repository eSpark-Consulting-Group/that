using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DDT.Application.Transitions
{
    public abstract class Transition
    {
        private Duration _duration = new Duration(TimeSpan.FromSeconds(1));
        public TransitionPresenter Owner { get; internal set; }

        protected static ResourceDictionary TransitionResources
        {
            get { return App.Current.Resources; }
        }

        public Duration Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        protected NameScope GetNameScope()
        {
            NameScope scope = new NameScope();
            NameScope.SetNameScope(Owner.TransitionContainer, scope);
            return scope;
        }

        public abstract void Setup(Brush prevBrush, Brush nextBrush);

        public abstract Storyboard PrepareStoryboard();
        public virtual void Cleanup() { }
    }
}