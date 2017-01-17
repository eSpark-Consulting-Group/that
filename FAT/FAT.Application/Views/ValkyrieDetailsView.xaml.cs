using System;
using System.Windows;

using FAT.Application.Models;
using APTAT.Common;

namespace FAT.Application
{
    public partial class ValkyrieDetailsView
    {
        public event EventHandler BackClick;

        public ValkyrieDetailsView()
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
