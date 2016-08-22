using DDT.Application.Extensions;
using System.Windows;

namespace DDT.Application.Views
{
    public partial class NetworkAddressesView
    {
        public NetworkAddressesView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            text.DelayedFocus();
        }
    }
}
