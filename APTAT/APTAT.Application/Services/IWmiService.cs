using System;
using APTAT.Core;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Application.Services
{
    public interface IWmiService
    {
        void StartAnalyze(DeploymentTarget[] items , String login, String password,
            DevicesSourceDescription devicesSourceDescription, Object userState);
        event EventHandler<WmiAnalyzeCompletedEventArgs> AnalyzeCompleted;
        event EventHandler<WmiAnalyzeProgressChangedEventArgs> ProgressChanged;
    }

    public class WmiAnalyzeCompletedEventArgs : EventArgs
    {
        public Guid OperationUid { get; set; }
    }
    public class WmiAnalyzeProgressChangedEventArgs : ScanInProgressEventArgs
    { }
}
