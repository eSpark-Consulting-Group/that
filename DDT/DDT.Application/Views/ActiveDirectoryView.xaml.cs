
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Telerik.Windows.Controls.GridView;

namespace DDT.Application.Views
{
    public partial class ActiveDirectoryView
    {
        public ActiveDirectoryView()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                BindingOperations.SetBinding(this, ExpandCommandProperty, new Binding("ExpandActiveDirectoryCommand"));
            }
        }

        #region ExpandCommandProperty (DependencyProperty)

        public ICommand ExpandCommand
        {
            get { return (ICommand)GetValue(ExpandCommandProperty); }
            set { SetValue(ExpandCommandProperty, value); }
        }

        public static readonly DependencyProperty ExpandCommandProperty =
            DependencyProperty.Register("ExpandCommand", typeof(ICommand), typeof(ActiveDirectoryView),
            new PropertyMetadata(null));

        #endregion

        private void RadTreeListView_RowIsExpandedChanging(object sender, RowCancelEventArgs e)
        {
            if (ExpandCommand.CanExecute(null))
                ExpandCommand.Execute(e.Row.DataContext);
        }
    }
}