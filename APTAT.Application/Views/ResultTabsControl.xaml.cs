using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for ResultTabsControl.xaml
    /// </summary>
    public partial class ResultTabsControl : UserControl
    {
        public ResultTabsControl()
        {
            InitializeComponent();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (e.Source as TabItem).IsSelected = true;
        }
    }
}
