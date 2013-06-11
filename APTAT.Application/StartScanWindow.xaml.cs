using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for StartScanWindow.xaml
    /// </summary>
    public partial class StartScanWindow 
    {
        public StartScanWindow()
        {
            InitializeComponent();

            
        }

        private void OnFinishClick(Object sender, EventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancelClick(Object sender, EventArgs e)
        {
            DialogResult = false;
        }
    }
}
