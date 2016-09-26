using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace APTAT.Application.Triggers
{
    public class KeyToCommandAction : TriggerAction<FrameworkElement>
    {
        #region CommandProperty (DependencyProperty)

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(KeyToCommandAction),
            new PropertyMetadata(null));

        #endregion

        #region CommandParameterProperty (DependencyProperty)

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(KeyToCommandAction),
            new PropertyMetadata(null));

        #endregion

        #region KeyProperty (DependencyProperty)

        public Key Key
        {
            get { return (Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(Key), typeof(KeyToCommandAction),
            new PropertyMetadata(Key.None));

        #endregion

        #region ModifierKeysProperty (DependencyProperty)

        public ModifierKeys ModifierKeys
        {
            get { return (ModifierKeys)GetValue(ModifierKeysProperty); }
            set { SetValue(ModifierKeysProperty, value); }
        }

        public static readonly DependencyProperty ModifierKeysProperty =
            DependencyProperty.Register("ModifierKeys", typeof(ModifierKeys), typeof(KeyToCommandAction),
            new PropertyMetadata(ModifierKeys.None));

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject != null && Key != Key.None)
            {
                AssociatedObject.AddHandler(UIElement.KeyUpEvent, (KeyEventHandler)element_KeyUp, true);
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler(UIElement.KeyUpEvent, (KeyEventHandler)element_KeyUp);

            base.OnDetaching();
        }

        protected override void Invoke(object parameter)
        {
        }

        private void element_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key && Keyboard.Modifiers == ModifierKeys && Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);
        }
    }
}
