using System;
using System.Windows;
using System.Windows.Controls;

namespace FAT.Application.Models
{
    public class BaseView : UserControl
    {
        public BaseView()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(Object sender, RoutedEventArgs e)
        {
            ViewModel viewModel = DataContext as ViewModel;
            if (viewModel != null)
            {
                viewModel.ViewLoadedCommand.Execute(null);
            }
        }

        private void OnUnloaded(Object sender, RoutedEventArgs e)
        {
            ViewModel viewModel = DataContext as ViewModel;
            if (viewModel != null)
            {
                viewModel.ViewUnloadedCommand.Execute(null);
            }  
        }
    }

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
