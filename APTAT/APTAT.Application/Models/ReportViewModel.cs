
using System;
using APTAT.Application.Services;
using System.Threading.Tasks;

namespace APTAT.Application.Models
{
    public class ReportViewModel : ViewModel
    {
        public event EventHandler ReportOpened;

        private readonly IStorageService _storage;
        private readonly IAptService _aptService;
        private String _header = String.Empty;
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

            if (kind == ReportKind.DetailedPerProgram || kind == ReportKind.DetailedPerProgramWithValkyrie)
            {
                Header = "Detailed Per-Program Report";
            }
            else if (kind == ReportKind.DetailedPerDevice || kind == ReportKind.DetailedPerDeviceWithValkyrie)
            {
                Header = "Detailed Per-Device Report";
            }
            else if (kind == ReportKind.Executive || kind == ReportKind.ExecutiveWithValkyrie)
            {
                Header = "Executive Report";
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

            ReportOpened?.Invoke(this, EventArgs.Empty);
        }
    }
}
