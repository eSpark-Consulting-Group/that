
using System;
using System.IO;
using System.Windows;
using APTAT.Application.Services;

namespace APTAT.Application.Models
{
    public class ReportViewModel : ViewModel
    {
        public event EventHandler ReportOpened;
        

        private IStorageService storageService = new StorageService();
        private IAptService _aptService = AptService.Instance;
        private String _header = String.Empty;

        public Stream ReportStream { get; set; }

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

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            Guid operationGuid = storageService.GetValue<Guid>(KnownKeys.OperationGuid, false);
            ReportKind kind = storageService.GetValue<ReportKind>(KnownKeys.ReportKind, false);

            if(kind == ReportKind.DetailedPerProgram)
            {
                Header = "Detailed Report Per Program";
            }
            else if(kind == ReportKind.DetailedPerDevice)
            {
                Header = "Detailed Report Per Device";
            }
            else if(kind == ReportKind.Executive)
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

            if(ReportStream != null)
            {
                ReportStream.Close();
            }
        }

        private void OnBuildReportCompleted(Object sender, BuildReportCompletedEventArgs e)
        {
            IsBusy = false;
            if(e.Exception != null)
            {
                MessageBox.Show(e.Exception.ToString());
                return;
            }

           // MessageBox.Show(e.FileName);


            try
            {
                FileName = e.FileName;
                ReportStream = File.Open(e.FileName, FileMode.Open);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());    
                throw;
            }
            


            if(ReportOpened != null)
            {
                ReportOpened(this, EventArgs.Empty);
            }
        }
    }
}
