using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FAT.Application
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

        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            var relativePoint = (sender as UserControl).TransformToAncestor(this).Transform(new Point(0, 0));

            if (relativePoint.Y > 33)
                grd1.Margin = grd2.Margin = new Thickness(-2);
        }
    }
}
