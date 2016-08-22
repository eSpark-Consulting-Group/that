using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DDT.Application.Transitions
{
    public class TransitionPresenter : ItemsControl
    {
        private Grid _childContainer;
        private FrameworkElement _nextChild;
        private FrameworkElement _prevChild;
        private bool _isPlaying;

        public static readonly DependencyProperty TransitionProperty;

        public Transition Transition
        {
            get { return (Transition)GetValue(TransitionProperty); }
            set { SetValue(TransitionProperty, value); }
        }

        internal Grid TransitionContainer { get; private set; }
        public Duration RestDuration { get; set; }

        public event EventHandler TransitionCompleted;

        static TransitionPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitionPresenter), new FrameworkPropertyMetadata( typeof(TransitionPresenter)));
            TransitionProperty = DependencyProperty.Register("Transition", typeof(Transition), typeof(TransitionPresenter), new PropertyMetadata(null));
        }

        public TransitionPresenter()
        {
        }

        public bool ApplyTransition(FrameworkElement prevChild, FrameworkElement nextChild)
        {
            if (_isPlaying) return false;

            _prevChild = prevChild;
            _nextChild = nextChild;
            _prevChild.Visibility = Transition == null ? Visibility.Collapsed : Visibility.Visible;
            _nextChild.Visibility = Visibility.Visible;

            if (Transition == null)
                return true;

            SwitchToTransitionMode(true);
            Transition.Owner = this;
            Transition.Setup(CreateBrush(_prevChild), CreateBrush(_nextChild));

            Storyboard sb = Transition.PrepareStoryboard();
            PlayStoryboard(sb);
            return true;
        }

        public void AddTransitionElement(FrameworkElement elt)
        {
            TransitionContainer.Children.Add(elt);
        }

        public override void OnApplyTemplate()
        {
            _childContainer = GetTemplateChild("PART_ChildrenContainer") as Grid;
            TransitionContainer = GetTemplateChild("PART_TransitionElementsContainer") as Grid;
            base.OnApplyTemplate();
        }

        private void PlayStoryboard(Storyboard sb)
        {
            EventHandler handler = null;
            handler = delegate
            {
                sb.Completed -= handler;
                FinishTransition();
            };
            sb.Completed += handler;
            sb.Begin(TransitionContainer);
        }

        private void SwitchToTransitionMode(bool isTransition)
        {
            if (isTransition)
            {
                TransitionContainer.Visibility = Visibility.Visible;
                _childContainer.Visibility = Visibility.Hidden;
            }
            else
            {
                TransitionContainer.Visibility = Visibility.Hidden;
                _childContainer.Visibility = Visibility.Visible;
            }

            _isPlaying = isTransition;
        }

        private static Brush CreateBrush(FrameworkElement elt)
        {
            VisualBrush brush = new VisualBrush(elt);
            brush.Viewbox = new Rect(0, 0, elt.ActualWidth, elt.ActualHeight);
            brush.ViewboxUnits = BrushMappingMode.Absolute;
            return brush;
        }

        private void ChangeChildrenStackOrder()
        {
            Panel.SetZIndex(_nextChild, 1);
            foreach (UIElement element in _childContainer.Children)
            {
                if (element != _nextChild)
                {
                    Panel.SetZIndex(element, 0);
                    element.Visibility = Visibility.Hidden;
                }
            }
        }

        public void FinishTransition()
        {
            Transition.Cleanup();
            TransitionContainer.Children.Clear();

            ChangeChildrenStackOrder();

            SwitchToTransitionMode(false);
            NotifyTransitionCompleted();
        }

        private void NotifyTransitionCompleted()
        {
            TransitionCompleted?.Invoke(this, null);

        }

        private FrameworkElement GetItem(int currentIndex)
        {
            return ItemContainerGenerator.ContainerFromIndex(currentIndex) as FrameworkElement;
        }
    }
}