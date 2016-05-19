using System;
using APTAT.Core;
using APTAT.Core.Descent;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using Composition = APTAT.Application.Helpers.CompositionHelper;

namespace APTAT.Application.Services
{
    [Composition.CreationPolicy(Composition.CreationPolicy.Singleton)]
    public class WmiService : IWmiService
    {
        public WmiService()
        {
            
        }

        public void StartAnalyze(DeploymentTarget[] items, String login, String password, Object userState)
        {
            Guid operationUid;
            var descentSubSystem = SubSystemFactory.Current.GetSubSystem<IDescentSubSystem>();
            descentSubSystem.StartWmiScan(items, login, password, out operationUid);
        }

        public event EventHandler<WmiAnalyzeCompletedEventArgs> AnalyzeCompleted;
        public event EventHandler<WmiAnalyzeProgressChangedEventArgs> ProgressChanged;
    }
}
