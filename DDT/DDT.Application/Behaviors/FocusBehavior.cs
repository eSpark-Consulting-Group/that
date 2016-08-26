using System.Windows.Interactivity;
using System.Windows;
using DDT.Application.Extensions;

namespace DDT.Application.Behaviors
{
    public class FocusBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += AssociatedObjectLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Loaded -= AssociatedObjectLoaded;
        }

        private void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.DelayedFocus();
        }
    }
}
