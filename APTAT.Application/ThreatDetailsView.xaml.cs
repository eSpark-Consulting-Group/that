using System;
using System.Windows;
using System.Windows.Input;
using APTAT.Application.Helpers;
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

            DataContext = _viewModel = CompositionHelper.Instance.GetService<ThreatDetailsViewModel>();
        }

        private void OnBackClick(Object sender, RoutedEventArgs e)
        {
           // NavigationService.GoBack();

            if(BackClick != null)
            {
                BackClick(this, EventArgs.Empty);
            }
            
        }

        private void BaseView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
