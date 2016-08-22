
using DDT.Application.Messages;
using System.Threading.Tasks;
using DDT.Application.Models;
using Telerik.Windows.Controls;
using System;

namespace DDT.Application.ViewModels
{
    public partial class MainViewModel
    {
        partial void InitializeReport()
        {
            GenerateReportCommand = new DelegateCommand(GenerateReport, CanGenerateReport);

            StopGenerateReportProgress = () => InternalProgress = 0;
        }

        public DelegateCommand GenerateReportCommand { get; private set; }

        public Action StopGenerateReportProgress { get; private set; }

        private string _reportPath;
        public string ReportPath
        {
            get { return _reportPath; }
            set
            {
                _reportPath = value;
                RaisePropertyChanged(nameof(ReportPath));
            }
        }

        private void GenerateReport(object o)
        {
            ButtonsAvailable = false;

            AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.Report, async _ =>
            {
                SetCurrentScreen(_);

                ReportPath = null;
                ReportPath = await TryAction(GenerateReport());

                if (string.IsNullOrEmpty(ReportPath))
                    InternalProgress = 0;
            });
        }

        private bool CanGenerateReport(object o)
        {
            return GenerateReportAvailable;
        }

        private async Task<string> GenerateReport()
        {
            ProgressMessage = "Generating Report...";
            InternalProgress = 100;

            DeploymentItem[] byIpAddress = new DeploymentItem[0];
            DeploymentItem[] byDomain = new DeploymentItem[0];
            DeploymentItem[] byWorkgroup = new DeploymentItem[0];

            foreach (var result in Results)
            {
                if (result.Type == DiscoveryTargetType.ActiveDirectory)
                {
                    byDomain = result.Items;
                }
                else if (result.Type == DiscoveryTargetType.NetworkAddress)
                {
                    byIpAddress = result.Items;
                }
                else if (result.Type == DiscoveryTargetType.Workgroup)
                {
                    byWorkgroup = result.Items;
                }
            }

            await Task.Delay(1500);

            return await _reportService.BuildDiscoveryAndDeploymentReport(byIpAddress, byDomain, byWorkgroup, _operation);
        }
    }
}
