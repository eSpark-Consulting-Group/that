using System;
using System.Windows;

using UFH.Application.Models;
using APTAT.Common;

namespace UFH.Application
{
    public partial class ThreatDetailsView
    {
        public event EventHandler BackClick;

        public ThreatDetailsView()
        {
            InitializeComponent();

            DataContext = CompositionHelper.Instance.Resolve<ThreatDetailsViewModel>();
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
