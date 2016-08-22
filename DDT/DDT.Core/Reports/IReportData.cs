using System;
using System.Collections.Generic;
using System.Linq;

namespace DDT.Core.Reports
{
    public interface IReportData
    {
        ChartData[] ChartDataByOS { get; }
        ChartData[] ChartDataByStatus { get; }
        TimeSpan ScanTime { get; set; }
        int FoundDevices { get; }
        int WindowsEndpoints { get; }
        int LinuxEndpoints { get; }
        int MacEndpoints { get; }
        int OthersDevices { get; }
        int StoppedDeployments { get; }
        int SuccessfulDeployments { get; }
        int FailedDeployments { get; }
        int SkippedDeployments { get; }
        IDeploymentReportModel DeploymentByDomain { get; }
        IDeploymentReportModel DeploymentByIPRange { get; }
        IDeploymentReportModel DeploymentByWorkgroup { get; }
        string OperationUid { get; set; }
    }
}
