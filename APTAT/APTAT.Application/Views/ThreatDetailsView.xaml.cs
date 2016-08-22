using System;
using System.Windows;
using APTAT.Application.Helpers;
using APTAT.Application.Models;
using APTAT.Common;

namespace APTAT.Application
{
    public partial class ThreatDetailsView
    {
        public event EventHandler BackClick;

        public ThreatDetailsView()
        {
            InitializeComponent();

            DataContext = CompositionHelper.Instance.GetService<ThreatDetailsViewModel>();
        }

        private void OnBackClick(Object sender, RoutedEventArgs e)
        {
            if(BackClick != null)
            {
                BackClick(this, EventArgs.Empty);
            }
            
        }
    }
}
