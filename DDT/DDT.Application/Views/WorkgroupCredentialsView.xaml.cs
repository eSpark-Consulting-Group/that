using DDT.Application.Extensions;
using System.Windows;

namespace DDT.Application.Views
{
    public partial class WorkgroupCredentialsView
    {
        public WorkgroupCredentialsView()
        {
            InitializeComponent();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            text.DelayedFocus();
        }
    }
}
