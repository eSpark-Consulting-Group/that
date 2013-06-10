using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using APTAT.Application.Models;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace APTAT.Application
{
    public static class DispatcherHelper
    {
        public static Dispatcher Current { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView 
    {
        private MainViewViewModel _viewModel;

        public MainView()
        {
            InitializeComponent();

            DataContext = _viewModel = new MainViewViewModel();
        }

      

        private void OnPropertiesClick(Object sender, RoutedEventArgs e)
        {
            ThreatDetailsWindow details = new ThreatDetailsWindow();
            details.Owner = App.Current.MainWindow;
            details.ShowDialog();
        }

        private void OnStartScanningClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/StartScanView.xaml", UriKind.Relative));
        }

        private void OnTreeDoubleClick(Object sender, MouseButtonEventArgs e)
        {
            if(e.OriginalSource != null)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                if(element != null && !(element.Parent is GridViewCell))
                {
                    return;
                }
            }

            if(_viewModel.SelectedItem != null && _viewModel.SelectedItem.IsRootItem == false)
            {
                ThreatDetailsWindow details = new ThreatDetailsWindow();
                details.Owner = System.Windows.Application.Current.MainWindow;
                details.ShowDialog();
            }
        }
    }
}
