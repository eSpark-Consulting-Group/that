using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace APTAT.Application.Models
{
    public class BasePage : Page
    {
        public BasePage()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(Object sender, RoutedEventArgs e)
        {
            ViewModel viewModel = DataContext as ViewModel;
            if (viewModel != null)
            {
                viewModel.ViewUnloadedCommand.Execute(null);
            }  
        }

        private void OnLoaded(Object sender, RoutedEventArgs e)
        {
            ViewModel viewModel = DataContext as ViewModel;
            if (viewModel != null)
            {
                viewModel.ViewLoadedCommand.Execute(null);
            }
        }
    }
}
