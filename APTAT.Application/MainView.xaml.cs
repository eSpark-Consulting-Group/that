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
using System.Diagnostics;

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
        //private Boolean _filteringDone = false;
        bool IsSearchPerformed = false;

        private List<ThreatInfoItem> _collapseItems = new List<ThreatInfoItem>();
        private List<ThreatInfoItem> _collapseFilteringItems = new List<ThreatInfoItem>();

        private MainViewViewModel _viewModel;
        private IStorageService _storageService;
        private Int32 _selectedReportIndex = -1;

        

        public MainView()
        {
            InitializeComponent();
            _storageService = new StorageService();
            DataContext = _viewModel = new MainViewViewModel();
            _viewModel.ScanProcessCompleted += OnScanCompleted;
            _viewModel.DomainTargetsResolveFailure += OnDomainTargetsResolveFailure;
            //treeList.Filtered += treeList_Filtered;
            //treeList.LayoutUpdated += treeList_LayoutUpdated;
            SearchTextBox.ValueChanged += SearchTextBox_ValueChanged;
           
        }

        void SearchTextBox_ValueChanged(object sender, RadRoutedEventArgs e)
        {
            //var searchTextBox = (APTAT.Application.Controls.RadMaskedTextBoxEx)sender;
            //IsSearchPerformed = !searchTextBox.IsEmpty;
            IsSearchPerformed = true;
        }

        //private void treeList_LayoutUpdated(object sender, EventArgs e)
        //{
        //    var rows = this.treeList.ChildrenOfType<GridViewRow>();
        //    foreach (GridViewRow row in rows)
        //    {
        //        if (row != null)
        //        {
        //            ThreatInfoItem threatInfo = row.DataContext as ThreatInfoItem;
        //            if (threatInfo != null)
        //            {
        //                row.IsExpandable = threatInfo.TempItems != null && threatInfo.TempItems.Count > 0 && threatInfo.ScanStage == ScanStage.OperationCompleted;
        //            }
        //        }
        //    }
        //}


        private void OnDomainTargetsResolveFailure(Object sender, DomainEventArgs e)
        {
            ErrorWindow error = new ErrorWindow();
            error.Text.Text = e.Exception.Message;
            error.Owner = System.Windows.Application.Current.MainWindow;
            error.ShowDialog();          
        }

        private void OnScanCompleted(Object sender, ScanCompletedEventArgs e)
        {
            
            ScanCompletedWindow scanCompletedWindow = new ScanCompletedWindow(_viewModel);
            scanCompletedWindow.Text.Text = String.Empty;
            if (e.SuccededCount > 0) scanCompletedWindow.Text.Text += String.Format("- {0} Successful Scans", e.SuccededCount);
            if (e.CanceledCount > 0) scanCompletedWindow.Text.Text += String.Format("{0}- {1} Canceled Scans", Environment.NewLine, e.CanceledCount);
            if (e.FailedCount > 0) scanCompletedWindow.Text.Text += String.Format("{0}- {1} Failed Scans", Environment.NewLine, e.FailedCount);
            if (e.NotPerformedCount > 0) scanCompletedWindow.Text.Text += String.Format("{0}- {1} Not Performed Scans", Environment.NewLine, e.NotPerformedCount);
            scanCompletedWindow.Text.Text += String.Format("{0}- {1} Total", Environment.NewLine, e.SuccededCount + e.CanceledCount + e.FailedCount + e.NotPerformedCount);
            if (e.MalwaresFound > 0) scanCompletedWindow.Text.Text += String.Format("{0}- {1} Malware(s) found", Environment.NewLine, e.MalwaresFound);
            scanCompletedWindow.Owner = System.Windows.Application.Current.MainWindow;
            if (scanCompletedWindow.ShowDialog() == true)
            {
                _viewModel.RunValkyrieAnalysis();
            }
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


        //private void treeList_RowIsExpandedChanged(object sender, RowEventArgs e)
        //{
        //    var row = e.Row as GridViewRow;
        //        if (row.IsExpanded)
        //        {
        //            if (row != null)
        //            {
        //                ThreatInfoItem item = ((ThreatInfoItem)row.DataContext);

        //                if (item.ScanStage == ScanStage.OperationCompleted)
        //                {
        //                    if (item.TempItems.Count > 0)
        //                    {
        //                        item.Items = item.TempItems;

        //                        //if (!IsSearchPerformed)
        //                        //{
        //                            foreach (ThreatInfoItem subItem in item.Items)
        //                            {
        //                                subItem.IsCompletedSubItem = true;
        //                            }
        //                        //}
        //                        //else
        //                        //    IsSearchPerformed = false;
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            ThreatInfoItem item = ((ThreatInfoItem)row.DataContext);
        //            if (item.Items != null)
        //            {
        //                foreach (ThreatInfoItem subItem in item.Items)
        //                {
        //                    subItem.IsCompletedSubItem = false;
        //                }
        //            }

        //            //item.Items = null;
        //        }            
        //}

        private void AboutClick(Object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.Owner = System.Windows.Application.Current.MainWindow;
            about.ShowDialog();
        }

        //private void OnRadGridViewDistinctValuesLoading(Object sender, GridViewDistinctValuesLoadingEventArgs e)
        //{
        //    e.ItemsSource = new[]
        //    {
        //        new ScanStatus(ScanStatusEnum.Cancelled),
        //        new ScanStatus(ScanStatusEnum.Completed),
        //        new ScanStatus(ScanStatusEnum.InProgress),
        //        new ScanStatus(ScanStatusEnum.Failed),
        //        new ScanStatus(ScanStatusEnum.Offline)
        //    };

        //}

        //private void treeList_Filtered(object sender, GridViewFilteredEventArgs e)
        //{
        //    treeList.CollapseAllHierarchyItems();
        //}

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
