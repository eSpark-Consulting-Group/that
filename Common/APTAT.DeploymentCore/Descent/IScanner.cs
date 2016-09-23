using System;
using APTAT.Common;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Messages;

namespace APTAT.DeploymentCore.Descent
{
    public interface IScanner : IDisposable
    {
        IDescentQueryResult ValidateAndGetResult();
        DescentEventArgs GetDescentCompletedEventArgs(IDescentQueryResult result);
        void StartScan(IChangeTrackingSubSystem changeTrackingSubSystem,IDescentQueryResult result);
        void AbortAllThread(int code = 0);
        FormattedText GetDescentStatusInformation(ResourceText errorFormat, DescentStatus status, int win32Error,
            string errorString, out FormattedText shortMessage);
        FormattedText GetDescentStatusDescription(DescentStatus status, out FormattedText shortMessage);

        void StopScan(Guid targetUid);

    }
}
