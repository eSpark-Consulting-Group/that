
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

    }
}
