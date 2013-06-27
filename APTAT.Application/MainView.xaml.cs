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
using APTAT.Application.Services;
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
            _viewModel.ScanProcessCompleted += OnScanCompleted;
        }

        private void OnScanCompleted(Object sender, ScanCompletedEventArgs e)
        {
            ScanCompletedWindow scanCompletedWindow = new ScanCompletedWindow();
            scanCompletedWindow.Text.Text = String.Format("- {0} Unknown Files Found", _viewModel.UnknownFilesCount);
            scanCompletedWindow.Text.Text += String.Format("{0}- {1} Malwares Found", Environment.NewLine, _viewModel.MalwareFilesCount);
            if(e.SuccededCount > 0)
            {
                scanCompletedWindow.Text.Text += String.Format("{0}- {1} Successful Scans", Environment.NewLine, e.SuccededCount);
            }
            if (e.CanceledCount > 0)
            {
                scanCompletedWindow.Text.Text += String.Format("{0}- {1} Cancelled Scans", Environment.NewLine, e.CanceledCount);
            }
            if (e.FailedCount > 0)
            {
                scanCompletedWindow.Text.Text += String.Format("{0}- {1} Failed Scans", Environment.NewLine, e.FailedCount);
            } 
            if (e.NotPerformedCount > 0)
            {
                scanCompletedWindow.Text.Text += String.Format("{0}- {1} Not Performed Scans", Environment.NewLine, e.NotPerformedCount);
            }
            scanCompletedWindow.Owner = System.Windows.Application.Current.MainWindow;
            scanCompletedWindow.ShowDialog();
        }
        private void OnPropertiesClick(Object sender, RoutedEventArgs e)
        {
            ThreatDetailsWindow details = new ThreatDetailsWindow();
            details.Owner = System.Windows.Application.Current.MainWindow;
            details.ShowDialog();
        }

        private void OnStartScanningClick(object sender, RoutedEventArgs e)
        {
            StartScanWindow details = new StartScanWindow();
            details.Owner = System.Windows.Application.Current.MainWindow;
            if(details.ShowDialog() == true)
            {
                _viewModel.StartScanCommand.Execute(null);
            }
        }

        private ThreatInfoItem GetDataBoundItem(FrameworkElement element)
        {
            if(element == null)
            {
                return null;
            }

            if(element.Parent == null)
            {
                return null;
            }

            FrameworkElement parent = (FrameworkElement) element.Parent;
            if(parent == null)
            {
                return null;
            }

            parent = (FrameworkElement) parent.Parent;
            if(parent == null)
            {
                return null;
            }

            return parent.DataContext as ThreatInfoItem;
        }

        private void OnTreeDoubleClick(Object sender, MouseButtonEventArgs e)
        {
            if(e.OriginalSource != null)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                if(element == null)
                {
                    return;
                }

                ThreatInfoItem item = GetDataBoundItem(element);
                if (_viewModel.SelectedItem != null && _viewModel.SelectedItem.IsRootItem == false && _viewModel.SelectedItem == item)
                {
                    ThreatDetailsWindow details = new ThreatDetailsWindow();
                    details.Owner = System.Windows.Application.Current.MainWindow;
                    details.ShowDialog();
                }
            }

           
        }

        private void OnBuildReportClick(Object sender, RoutedEventArgs e)
        {
            ReportWindow details = new ReportWindow();
            details.Owner = System.Windows.Application.Current.MainWindow;
            details.ShowDialog();
        }
    }
}
