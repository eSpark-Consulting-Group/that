using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DDT.Application.Views
{
    public partial class DiscoveryTargetsView
    {
        public DiscoveryTargetsView()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                BindingOperations.SetBinding(this, CommandProperty, new Binding("ChangeScreenCommand"));
            }
        }

        #region CommandProperty (DependencyProperty)

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(DiscoveryTargetsView),
            new PropertyMetadata(null));

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button)
            {
                var button = e.OriginalSource as Button;
                if (button.Name == "close")
                {
                    button.Command.Execute(null);
                    return;
                }
            }

            Command.Execute((sender as Button).CommandParameter);
        }
    }
}