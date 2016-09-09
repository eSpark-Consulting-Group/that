using APTAT.DeploymentCore;
using APTAT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDT.Core.Reports
{

    public class ReportData : IReportData
    {
        public ReportData()
        {
            DeploymentByDomain = new DeploymentReportModel(new List<DetailsOfScan>());
            DeploymentByIPRange = new DeploymentReportModel(new List<DetailsOfScan>());
            DeploymentByWorkgroup = new DeploymentReportModel(new List<DetailsOfScan>());
            _allItems = new DetailsOfScan[] { };
        }

        public ChartData[] ChartDataByOS
        {
            get
            {
                var data = new[] {
                    new ChartData() { Name = "Windows", Count = WindowsEndpoints == 0? (int?) null: WindowsEndpoints },
                    new ChartData() { Name = "Mac", Count = MacEndpoints == 0? (int?) null: MacEndpoints },
                    new ChartData() { Name = "Linux", Count = LinuxEndpoints == 0? (int?)null: LinuxEndpoints },
                    new ChartData() { Name = "Others", Count = OthersDevices == 0? (int?) null: OthersDevices }
                };

                return data;
            }
        }

        public ChartData[] ChartDataByStatus
        {
            get
            {
                var data = new[] {
                    new ChartData() { Name = "Successful", Count = (SuccessfulDeployments==0)?(int?)null: SuccessfulDeployments },
                    new ChartData() { Name = "Stopped", Count = (StoppedDeployments ==0)?(int?)null: StoppedDeployments },
                    new ChartData() { Name = "Skipped", Count = (SkippedDeployments==0)?(int?)null: SkippedDeployments },
                    new ChartData() { Name = "Failed", Count = (FailedDeployments==0)?(int?)null: FailedDeployments }
                };

                return data;
            }
        }


        private readonly IDetailsOfScan[] _allItems;

        public TimeSpan ScanTime { get; set; }
        public int FoundDevices { get; }
        public int WindowsEndpoints { get; }
        public int LinuxEndpoints { get; }
        public int MacEndpoints { get; }
        public int OthersDevices { get; }
        public int StoppedDeployments { get; }
        public int SuccessfulDeployments { get; }
        public int FailedDeployments { get; }
        public int SkippedDeployments { get; }
        public IDeploymentReportModel DeploymentByDomain { get; private set; }
        public IDeploymentReportModel DeploymentByIPRange { get; private set; }
        public IDeploymentReportModel DeploymentByWorkgroup { get; private set; }
     
        public string OperationUid { get; set; }

        public ReportData(IDeploymentReportModel deploymentByWorkgroup, IDeploymentReportModel deploymentByIPRange, IDeploymentReportModel deploymentByDomain)
        {
            DeploymentByDomain = deploymentByDomain;
            DeploymentByIPRange = deploymentByIPRange;
            DeploymentByWorkgroup = deploymentByWorkgroup;
            _allItems = this.DeploymentByDomain.Details
                .Union(this.DeploymentByIPRange.Details)
                .Union(this.DeploymentByWorkgroup.Details)
                .DistinctBy(i=>i.TargetUid).ToArray();

            FoundDevices = _allItems.Count();
            WindowsEndpoints = _allItems.Count(i => i.SystemFamilyKind == SystemFamilyKind.Windows);
            OthersDevices = _allItems.Count(i => i.SystemFamilyKind == SystemFamilyKind.Unknown);
            MacEndpoints = _allItems.Count(i => i.SystemFamilyKind == SystemFamilyKind.MacOS);
            LinuxEndpoints = _allItems.Count(i => i.SystemFamilyKind == SystemFamilyKind.Linux);

            SkippedDeployments = _allItems.Count(d => d.DeploymentStatus == DeploymentStatus.Skipped);
            StoppedDeployments = _allItems.Count(d => d.DeploymentStatus == DeploymentStatus.Stopped);
            FailedDeployments = _allItems.Count(d => d.DeploymentStatus == DeploymentStatus.Failed);
            SuccessfulDeployments = _allItems.Count(d => d.DeploymentStatus == DeploymentStatus.Succeded);            
        }
    }
}
