using System;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Application.Services
{
    public interface IWmiService
    {
        void StartAnalyze(DeploymentTarget[] items , String login, String password, Object userState);
        event EventHandler<WmiAnalyzeCompletedEventArgs> AnalyzeCompleted;
        event EventHandler<WmiAnalyzeProgressChangedEventArgs> ProgressChanged;
    }
    public class WmiAnalyzeCompletedEventArgs : EventArgs
    { }
    public class WmiAnalyzeProgressChangedEventArgs : EventArgs
    { }
}
