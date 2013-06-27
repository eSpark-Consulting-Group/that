
using System;
using System.IO;
using APTAT.Application.Services;

namespace APTAT.Application.Models
{
    public class ReportViewModel : ViewModel
    {
        public event EventHandler ReportOpened;

        private IStorageService storageService = new StorageService();
        private IAptService _aptService = new AptService();

        public Stream ReportStream { get; set; }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            Guid operationGuid = storageService.GetValue<Guid>(KnownKeys.OperationGuid, false);
            ReportKind kind = storageService.GetValue<ReportKind>(KnownKeys.ReportTKind, false);


            _aptService.BuildReportCompleted += OnBuildReportCompleted;
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
            if(e.Exception != null)
            {
                return;
            }

            ReportStream = File.Open(e.FileName, FileMode.Open);
            if(ReportOpened != null)
            {
                ReportOpened(this, EventArgs.Empty);
            }
        }
    }
}
