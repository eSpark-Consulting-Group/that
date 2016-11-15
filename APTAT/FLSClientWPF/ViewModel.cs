using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using APTAT.Core;


namespace FLSClientWPF
{
    class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<FlsResult> FlsResults { get; set; }
        private readonly CancellationTokenSource _cancellationTokenSource;

        private string _progress;
        private double _progressValue;
        private readonly FlsManager _manager;

        public double ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }

        public ViewModel()
        {
            FlsResults = new ObservableCollection<FlsResult>();
            _cancellationTokenSource = new CancellationTokenSource();

            var hostName = "fls.security.comodo.com";
            IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
            if (!hostEntry.AddressList.Any()) throw new InvalidOperationException("Could not resolve host " + hostName);
            var ip = hostEntry.AddressList[0];

            _manager = new FlsManager();
            _manager.FlsAnalyzed += ManagerFlsAnalyzed;

        }

        private void ManagerFlsAnalyzed(object sender, FlsRequestEventArgs e)
        {
           if(e.Exception != null) HandleError(e.Exception);
           ProgressReport(e.Results);
        }

        private static void HandleError(Exception e)
        {
            MessageBox.Show(e.Message);
        }
     

        private void ProgressReport(FlsResult[] results)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                    {
                        foreach (var r in results)
                        {
                            FlsResults.Add(r);
                        }

                    }));
        }

        public string Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Cancel()
        {
            if (_cancellationTokenSource != null && _cancellationTokenSource.Token.CanBeCanceled)
                _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            
        }

        public async Task Add()
        {
            var hashes = (new HashData()).Hashes;
           await _manager.Enqueue(hashes);
        }
    }
}
