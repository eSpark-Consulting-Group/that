
using FAT.Application.Messages;
using System.Windows.Controls;

namespace FAT.Application
{
    public partial class DiscoveryResults : UserControl
    {
        public DiscoveryResults()
        {
            InitializeComponent();

            DataContext = App.Current.MainWindow.DataContext;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AppMessages.FilterMessage.Send(MessageTypes.FilterTarget, _ => _.Target = treeList);
        }
    }
}
