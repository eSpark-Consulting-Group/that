
using System;
using System.IO;
using System.Windows;
using APTAT.Application.Services;

namespace APTAT.Application.Models
{
    public class ReportViewModel : ViewModel
    {
        public event EventHandler ReportOpened;
        

        private IStorageService _storage;
        private IAptService _aptService;
        private String _header = String.Empty;

        //public Stream ReportStream { get; set; }

        public String FileName { get; set; }

        public String Header
        {
            get { return _header; }
            set
            {
                _header = value;
                RaisePropertyChanged("Header");
            }
        }

        public ReportViewModel(IAptService aptService, IStorageService storage)
        {
            _aptService = aptService;
            _storage = storage;
        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);
            
            ReportKind kind = _storage.GetValue<ReportKind>(KnownKeys.ReportKind, false);

            if(kind == ReportKind.DetailedPerProgram || kind == ReportKind.DetailedPerProgramWithValkyrie)
            {
                Header = "Detailed Per-Program Report";
            }
            else if(kind == ReportKind.DetailedPerDevice || kind == ReportKind.DetailedPerDeviceWithValkyrie)
            {
                Header = "Detailed Per-Device Report";
            }
            else if(kind == ReportKind.Executive || kind == ReportKind.ExecutiveWithValkyrie)
            {
                Header = "Executive Report";
            }

            _aptService.BuildReportCompleted += OnBuildReportCompleted;
            IsBusy = true;
            _aptService.BuildReport(kind, this);
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
            if(e.Exception != null)
            {
                if (e.Exception is OutOfMemoryException)
                {
                    Error = e.Exception.Message;
                    HasErrors = true;
                }
                
                return;
            }

           // MessageBox.Show(e.FileName);


            //try
            //{
                FileName = e.FileName;
            //ReportStream = File.Open(e.FileName, FileMode.Open);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());    
            //    throw;
            //}



            ReportOpened?.Invoke(this, EventArgs.Empty);
        }
    }
}
