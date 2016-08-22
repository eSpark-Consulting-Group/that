using System;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Messages;

namespace APTAT.Core.Descent.Apt
{
    public class AptDescentProgressItem : DescentProgressItem,IAptDescentProgressItem
    {
        public AptDescentProgressItem(Guid computerUid, string hostName, Guid operationUid, FormattedText statusMessage, AptScanStage scanStage, double percentage)
            : this(computerUid, hostName, operationUid, statusMessage, scanStage, 0, 0, 0, 0, percentage, 0, 0)
        {
        }

        public AptDescentProgressItem(Guid computerUid, string hostName, Guid operationUid, FormattedText statusMessage, AptScanStage scanStage, int totalFilesCount, int unknownFilesCount, double percentage, int errorFilesCount, int failedAtNodeCount)
            : this(computerUid, hostName, operationUid, statusMessage, scanStage, totalFilesCount, unknownFilesCount, 0, 0, percentage, errorFilesCount, failedAtNodeCount)
        {
        }

        public AptDescentProgressItem(Guid computerUid, string hostName, Guid operationUid, FormattedText statusMessage, AptScanStage scanStage, 
            int totalFilesCount, int unknownFilesCount, int malwaresCount, int safeFilesCount, double percentage, int errorFileCount,
            int failedAtNodeCount)
            :base (computerUid, hostName, operationUid, statusMessage, percentage, scanStage == AptScanStage.OperationFailed, 
                 scanStage == AptScanStage.OperationCompleted || scanStage == AptScanStage.OperationFailed || scanStage == AptScanStage.OperationCanceled
                 || scanStage == AptScanStage.ScanFailed) 
        {
            ScanStage = scanStage;
            TotalFilesCount = totalFilesCount;
            UnknownFilesCount = unknownFilesCount;
            MalwaresCount = malwaresCount;
            SafeFilesCount = safeFilesCount;
            ErrorFilesCount = errorFileCount;
            FailedAtNodeCount = failedAtNodeCount;
        }


        #region IDescentProgressItem Members
        public int FailedAtNodeCount { get; set; }
        public AptScanStage ScanStage { get; private set; }
        public int TotalFilesCount { get; private set; }
        public int UnknownFilesCount { get; private set; }
        public int MalwaresCount { get; private set; }
        public int SafeFilesCount { get; private set; }
        public int ErrorFilesCount { get; private set; }
        #endregion
    }
}
