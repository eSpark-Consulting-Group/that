using System;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core
{
    public class ComputerScanCompletedEventArgs : EventArgs
    {
        public ComputerScanCompletedEventArgs(Guid operationUid, IAptScanResult completedResult)
        {
            OperationUid = operationUid;
            CompletedResult = completedResult;
        }

        public Guid OperationUid { get;  }
        public IAptScanResult CompletedResult { get;  }
    }
}