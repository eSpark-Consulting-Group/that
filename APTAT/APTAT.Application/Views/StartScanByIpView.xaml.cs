
using System.Windows.Controls;
using System.Windows.Input;
using APTAT.Application.Extensions;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for StartScanByIpView.xaml
    /// </summary>
    public partial class StartScanByIpView : UserControl
    {
        public StartScanByIpView()
        {
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            PortValidationExtensions.ValidateInput(sender, e);
        }
    }
}
