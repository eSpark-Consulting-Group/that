using System.Collections.Generic;
using System.Linq;

namespace DDT.Core.Reports
{
    public interface IDeploymentReportModel
    {
        IEnumerable<IDetailsOfScan> Details { get; }
        int FoundDevices { get; }
        int WindowsEndpoints { get; }
        int LinuxEndpoints { get; }
        int MacEndpoints { get; }
        int OthersDevices { get; }
        int SuccessfulDeployments { get; }
        int FailedDeployments { get; }
        int SkippedDeployments { get; }
        int StoppedDeployments { get; }
    }
}
