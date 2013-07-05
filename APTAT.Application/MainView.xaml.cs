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
using Telerik.Windows;
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
        private IStorageService _storageService;
        private Int32 _selectedReportIndex = -1;

        public MainView()
        {
            InitializeComponent();
            _storageService = new StorageService();
            DataContext = _viewModel = new MainViewViewModel();
            _viewModel.ScanProcessCompleted += OnScanCompleted;
  
        }



        private void OnScanCompleted(Object sender, ScanCompletedEventArgs e)
        {
            ScanCompletedWindow scanCompletedWindow = new ScanCompletedWindow();
            scanCompletedWindow.Text.Text = String.Empty;
            scanCompletedWindow.Text.Text += String.Format("- {0} Successful Scans", e.SuccededCount);
            scanCompletedWindow.Text.Text += String.Format("{0}- {1} Cancelled Scans", Environment.NewLine, e.CanceledCount);
            scanCompletedWindow.Text.Text += String.Format("{0}- {1} Failed Scans", Environment.NewLine, e.FailedCount);
            scanCompletedWindow.Text.Text += String.Format("{0}- {1} Not Performed Scans", Environment.NewLine, e.NotPerformedCount);
            scanCompletedWindow.Text.Text += String.Format("{0}- {1} Total", Environment.NewLine, e.SuccededCount + e.CanceledCount + e.FailedCount + e.NotPerformedCount);
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



        private static void ShowReport()
        {
            ReportWindow details = new ReportWindow();
            details.Owner = System.Windows.Application.Current.MainWindow;
            details.ShowDialog();
        }

  

        private void OnExecutiveReportClick(Object sender, RadRoutedEventArgs e)
        {
            _storageService.SetValue(KnownKeys.ReportKind, ReportKind.Executive);
            ReportButton.IsOpen = false;
            ShowReport();
        }

        private void OnPerDeviceReportClick(Object sender, RadRoutedEventArgs e)
        {
            _storageService.SetValue(KnownKeys.ReportKind, ReportKind.DetailedPerDevice);
            ReportButton.IsOpen = false;
            ShowReport();
        }

        private void OnPerProgramReportClick(Object sender, RadRoutedEventArgs e)
        {
            _storageService.SetValue(KnownKeys.ReportKind, ReportKind.DetailedPerProgram);
            ReportButton.IsOpen = false;
            ShowReport();
            
        }

        private void OnStopScanClick(Object sender, RoutedEventArgs e)
        {
            ConfirmationWindow confirmation = new ConfirmationWindow();
            confirmation.Owner = System.Windows.Application.Current.MainWindow;
            
            if(confirmation.ShowDialog() == true)
            {
                _viewModel.StopScanCommand.Execute(null);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            treeList.Rebind();
        }

     
        private void treeList_RowLoaded(object sender, RowLoadedEventArgs e)
        {
            var row = e.Row as GridViewRow;
            if (row != null)
            {
                ThreatInfoItem threatInfo = (ThreatInfoItem)row.DataContext;
                row.IsExpandable = threatInfo.HasBadFiles && threatInfo.ScanStage == ScanStage.OperationCompleted;
            }
        }

        private void treeList_RowIsExpandedChanged(object sender, RowEventArgs e)
        {
            var row = e.Row;
            if(row != null)
            {
                ThreatInfoItem threatInfo = (ThreatInfoItem)row.DataContext;
                threatInfo.Items = threatInfo.Items;
            }
        }

        private void AboutClick(Object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.Owner = System.Windows.Application.Current.MainWindow;
            about.ShowDialog();
        }
    }
}
