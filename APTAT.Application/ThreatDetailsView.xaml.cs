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
using System.Windows.Navigation;
using System.Windows.Shapes;
using APTAT.Application.Models;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for ThreatProperties.xaml
    /// </summary>
    public partial class ThreatDetailsView
    {
        public event EventHandler BackClick;
        private ThreatDetailsViewModel _viewModel;

        public ThreatDetailsView()
        {
            InitializeComponent();

            DataContext = _viewModel = new ThreatDetailsViewModel();
        }

        private void OnBackClick(Object sender, RoutedEventArgs e)
        {
           // NavigationService.GoBack();

            if(BackClick != null)
            {
                BackClick(this, EventArgs.Empty);
            }
            
        }
    }
}
