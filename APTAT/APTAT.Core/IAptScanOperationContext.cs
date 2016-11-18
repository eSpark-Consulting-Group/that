using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Descent.Wmi;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;

namespace APTAT.Core.Descent
{
    public interface IAptScanOperationContext : IDescentQueryResult
    {
        int SucceededTargetsCount { get;  }
        int FailedTargetsCount { get;  }
        int NotPerformedTargetsCount { get;  }
        int CanceledTargetsCount { get;  }
        DateTime Started { get; }
        IEnumerable<IAptScanResult> ScanResults { get; }

        event EventHandler<AptOperationProgressChangedEventArgs> OperationProgressChanged;
        IFilesStatistics CalculateFilesStatistics();
        IAptScanResult GetAptResultByTargetUid(Guid targetUid);
    }

    public class AptOperationProgressChangedEventArgs : OperationProgressChangedEventArgs
    {
        public IDescentProgressItem[] ProgressItems { get; set; }
    }
    public interface IWmiScanOperationContext : IDescentQueryResult
    {
        ConcurrentDictionary<Guid, WmiScanResult> ScanResults { get; }
    }
}
