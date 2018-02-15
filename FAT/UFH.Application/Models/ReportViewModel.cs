
using System;
using UFH.Application.Services;
using System.Threading.Tasks;
using APTAT.Core.CMC.Request;
using static UFH.Application.Messages.AppMessages;

namespace UFH.Application.Models
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

        public APTAT.Core.CMC.ReportKind ReportKind
        {
            get; private set;
        }

        public string FileName { get; private set; }

        public ReportViewModel(IAptService aptService, IStorageService storage)
        {
            _aptService = aptService;
            _storage = storage;
        }

        protected override  void  InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            ReportKind kind = _storage.GetValue<ReportKind>(KnownKeys.ReportKind, false);

            if (kind == UFH.Application.Services.ReportKind.DetailedPerProgram || kind == UFH.Application.Services.ReportKind.DetailedPerProgramWithValkyrie)
            {
                Header = "Per-Program Report";
                ReportKind = APTAT.Core.CMC.ReportKind.PerProgram;
            }
            else if (kind == UFH.Application.Services.ReportKind.DetailedPerDevice || kind == UFH.Application.Services.ReportKind.DetailedPerDeviceWithValkyrie)
            {
                Header = "Per-Device Report";
                ReportKind = APTAT.Core.CMC.ReportKind.PerDevice;
            }
            else if (kind == UFH.Application.Services.ReportKind.Executive || kind == UFH.Application.Services.ReportKind.ExecutiveWithValkyrie)
            {
                Header = "Executive Report";
                ReportKind = APTAT.Core.CMC.ReportKind.Executive;
            }
                     
            IsBusy = true;

            Task.Run(async () =>
            {
                var res = await _aptService.BuildReportAsync(kind);
                OnBuildReportCompleted(res);
            });
            
        }

        private void OnBuildReportCompleted(BuildReportResult e)
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
            if (e.IsGenerated)
                CMCStatisticsMessages<IReportGeneratedRequest>.Send(request =>
                {       
                    request.ReportKind = ReportKind;
                });
        }

        public void OnSaveReportCompleted()
        {
            CMCStatisticsMessages<IReportPrintedOrSavedRequest>.Send(request =>
            {
                request.ReportKind = ReportKind;
                request.ReportAction = APTAT.Core.CMC.ReportAction.Saved;
            });
        }

        public void OnPrintReportCompleted()
        {
            CMCStatisticsMessages<IReportPrintedOrSavedRequest>.Send(request =>
            {
                request.ReportKind = ReportKind;
                request.ReportAction = APTAT.Core.CMC.ReportAction.Printed;
            });
        }
    }
}
