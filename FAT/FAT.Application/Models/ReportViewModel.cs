
using System;
using System.IO;
using System.Windows;
using FAT.Application.Services;

namespace FAT.Application.Models
{
    public class ReportViewModel : ViewModel
    {
        public event EventHandler ReportOpened;        
        private IStorageService _storage;
        private IAptService _aptService;
        private String _header = String.Empty;
                
        public byte[] DocumentBytes { get; set; }

        public String Header
        {
            get { return _header; }
            set
            {
                _header = value;
                RaisePropertyChanged("Header");
            }
        }

        public string FileName { get; private set; }

        public ReportViewModel(IAptService aptService, IStorageService storage)
        {
            _aptService = aptService;
            _storage = storage;
        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            Guid operationGuid = _storage.GetValue<Guid>(KnownKeys.OperationGuid, false);
            ReportKind kind = _storage.GetValue<ReportKind>(KnownKeys.ReportKind, false);

            if (kind == ReportKind.DetailedPerProgram || kind == ReportKind.DetailedPerProgramWithValkyrie)
            {
                Header = "Per-Program Report";
            }
            else if (kind == ReportKind.DetailedPerDevice || kind == ReportKind.DetailedPerDeviceWithValkyrie)
            {
                Header = "Per-Device Report";
            }
            else if (kind == ReportKind.Executive || kind == ReportKind.ExecutiveWithValkyrie)
            {
                Header = "Executive Report";
            }

            _aptService.BuildReportCompleted += OnBuildReportCompleted;
            IsBusy = true;
            _aptService.BuildReport(operationGuid, kind, this);
        }

        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _aptService.BuildReportCompleted -= OnBuildReportCompleted;

            //if(ReportStream != null)
            //{
            //    ReportStream.Close();
            //}
        }

        private void OnBuildReportCompleted(Object sender, BuildReportCompletedEventArgs e)
        {

            IsBusy = false;
            if (e.Exception != null)
            {
                if (e.Exception is OutOfMemoryException)
                {
                    Error = e.Exception.Message;
                    HasErrors = true;
                }

                return;
            }            

            FileName = e.FileName;
            DocumentBytes = e.DocumentBytes;
            
            ReportOpened?.Invoke(this, EventArgs.Empty);
        }
    }
}
