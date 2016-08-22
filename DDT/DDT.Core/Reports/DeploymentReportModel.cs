using APTAT.DeploymentCore;
using System.Collections.Generic;
using System.Linq;

namespace DDT.Core.Reports
{
    public class DeploymentReportModel : IDeploymentReportModel
    {
        public DeploymentReportModel(IEnumerable<IDetailsOfScan> details)
        {
            Details = details.ToList();
        }
        public IEnumerable<IDetailsOfScan> Details { get; private set; }
        
        public int FoundDevices
        {
            get
            {
                return Details.Count();
            }
        }

        public int WindowsEndpoints
        {
            get
            {
                return Details.Count(i => i.SystemFamilyKind == SystemFamilyKind.Windows);
            }
        }

        public int LinuxEndpoints
        {
            get
            {
                return Details.Count(i => i.SystemFamilyKind == SystemFamilyKind.Linux);
            }
        }        

        public int MacEndpoints
        {
            get
            {
                return Details.Count(i => i.SystemFamilyKind == SystemFamilyKind.MacOS);
            }
        }
        public int OthersDevices
        {
            get
            {
                return Details.Count(i => i.SystemFamilyKind == SystemFamilyKind.Unknown);
            }
        }

        public int SuccessfulDeployments
        {
            get
            {
                return Details.Count(d => d.DeploymentStatus == DeploymentStatus.Succeded);
            }
        }
        public int FailedDeployments
        {
            get
            {
                return Details.Count(d => d.DeploymentStatus == DeploymentStatus.Failed);
            }
        }
        public int SkippedDeployments
        {
            get
            {
                return Details.Count(d => d.DeploymentStatus == DeploymentStatus.Skipped);
            }
        }

        public int StoppedDeployments
        {
            get
            {
                return Details.Count(d => d.DeploymentStatus == DeploymentStatus.Stoped);
            }
        }
    }
}
