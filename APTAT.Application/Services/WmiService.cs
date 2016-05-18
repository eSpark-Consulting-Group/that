using System;
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
        { }

        public event EventHandler<WmiAnalyzeCompletedEventArgs> AnalyzeCompleted;
    }
}
