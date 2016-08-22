using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace DDT.Application.Behaviors
{
    public class TextPatternBehavior : Behavior<TextBox>
    {
        #region AllowIntProperty (DependencyProperty)

        public bool AllowInt
        {
            get { return (bool)GetValue(AllowIntProperty); }
            set { SetValue(AllowIntProperty, value); }
        }

        public static readonly DependencyProperty AllowIntProperty = 
            DependencyProperty.Register("AllowInt", typeof(bool), typeof(TextPatternBehavior), 
            new PropertyMetadata(false));

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.KeyDown += AssociatedObjectKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.KeyDown -= AssociatedObjectKeyDown;
        }

        private void AssociatedObjectKeyDown(object sender, KeyEventArgs e)
        {
            bool flag = AllowInt && (((e.Key >= Key.NumPad0) && (e.Key <= Key.NumPad9)) || ((e.Key >= Key.D0) && (e.Key <= Key.D9)));

            if ((e.Key == Key.Tab) || (e.Key == Key.Enter))
                flag = true;

            if (!flag)
                e.Handled = true;
        }
    }
}
