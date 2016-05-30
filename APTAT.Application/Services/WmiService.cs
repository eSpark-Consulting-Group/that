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
        public void StartAnalyze(DeploymentTarget[] items, String login, String password, 
            DevicesSourceDescription devicesSourceDescription, Object userState)
        {
            Guid operationUid;
            var descentSubSystem = SubSystemFactory.Current.GetSubSystem<IDescentSubSystem>();
            descentSubSystem.DescentCompleted += DescentSubSystemOnDescentCompleted;
            try
            {
                descentSubSystem.StartWmiScan(items, login, password, devicesSourceDescription, out operationUid);
            }
            catch (Exception)
            {
                descentSubSystem.DescentCompleted -= DescentSubSystemOnDescentCompleted;
                throw;
            }
            var scanOperationContext = descentSubSystem.GetWmiScanResultsByOperationUid(operationUid);
            if (scanOperationContext != null)
            {
                scanOperationContext.OperationProgressChanged += ScanOperationContextOnOperationProgressChanged;
            }
        }

        private void DescentSubSystemOnDescentCompleted(object sender, DescentEventArgs e)
        {
            var descentSubSystem = SubSystemFactory.Current.GetSubSystem<IDescentSubSystem>();
            descentSubSystem.DescentCompleted -= DescentSubSystemOnDescentCompleted;
            var scanOperationContext = descentSubSystem.GetWmiScanResultsByOperationUid(e.OperationUid);
            if (scanOperationContext != null)
            {
                scanOperationContext.OperationProgressChanged -= ScanOperationContextOnOperationProgressChanged;
            }
            var tmp = AnalyzeCompleted;
            if (tmp != null)
            {
                tmp.Invoke(this, new WmiAnalyzeCompletedEventArgs() { OperationUid = e.OperationUid });
            }
        }

        private void ScanOperationContextOnOperationProgressChanged(object sender, OperationProgressChangedEventArgs e)
        {
            var tmp = ProgressChanged;
            if (tmp != null)
            {
                tmp.Invoke(this, new WmiAnalyzeProgressChangedEventArgs() {OperationUid = e.OperationUid, Percentage = e.Percentage});
            }
        }

        public event EventHandler<WmiAnalyzeCompletedEventArgs> AnalyzeCompleted;
        public event EventHandler<WmiAnalyzeProgressChangedEventArgs> ProgressChanged;
    }
}
