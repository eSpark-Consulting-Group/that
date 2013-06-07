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
        //private ObservableCollection<ThreatInfoItem> _threats;

        //private static ObservableCollection<ThreatInfoItem>  GetThreats()
        //{
            
        //    ObservableCollection<ThreatInfoItem> threatInfoItems = new ObservableCollection<ThreatInfoItem>();
        //    for(Int32 i = 0; i < 100; i++)
        //    {
        //        ThreatInfoItem c = new ThreatInfoItem
        //                               {
        //                                   Id = i,
        //                                   Items = new ObservableCollection<ThreatInfoItem>(),
        //                                   Name = "Computer " + i,
        //                                   Size = String.Empty,
        //                                   Type = String.Empty,
        //                                   IsRootItem = true
        //                               };
        //        threatInfoItems.Add(c);
               
        //    }

        //    return threatInfoItems;
        //}

        public MainView()
        {
            InitializeComponent();

            DataContext = _viewModel = new MainViewViewModel();

            //DispatcherHelper.Current = Dispatcher;

            //_threats = GetThreats();
            //treeList.ItemsSource = _threats;

            //BackgroundWorker worker = new BackgroundWorker();
            //worker.WorkerReportsProgress = true;
            //worker.DoWork += worker_DoWork;
            //worker.ProgressChanged += worker_ProgressChanged;
            //worker.RunWorkerAsync();
        }

       // Random rand = new Random(DateTime.Now.Millisecond);

        //private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //    Int32 index = rand.Next() % _threats.Count;
        //    ThreatInfoItem threat = _threats[index];
        //    threat.Progress += 5;
        //    if (threat.Progress >= 100)
        //    {
        //        threat.Progress = 100;
        //        _threats.Remove(threat);
        //    }
        //    else
        //    {
        //        Double val = rand.NextDouble();
        //        if(val < 0.33)
        //        {
        //            ThreatInfoItem t = new ThreatInfoItem
        //            {
        //                Id = 0,
        //                Items = new ObservableCollection<ThreatInfoItem>(),
        //                Name = "Unknown" + rand.Next() + ".exe",
        //                Size = "8019KB",
        //                Type = "Application.exe"
        //            };
        //            threat.Items.Add(t);
        //        }
        //    }
        //}

        //private void worker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    while (_threats.Count > 0)
        //    {
      
        //        BackgroundWorker worker = (BackgroundWorker) sender;
        //        worker.ReportProgress(0);

        //        Thread.Sleep(100);
        //    }
        //}

        private void OnPropertiesClick(Object sender, RoutedEventArgs e)
        {
           // NavigationService.Navigate(new Uri("/ThreatDetailsView.xaml", UriKind.Relative));

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
            ThreatDetailsWindow details = new ThreatDetailsWindow();
            details.Owner = App.Current.MainWindow;
            details.ShowDialog();
        }
    }
}
