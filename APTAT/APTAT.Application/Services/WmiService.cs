using System;
using APTAT.Common;
using APTAT.Core;
using APTAT.Core.Descent.Wmi;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Descent.Exceptions;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using Composition = APTAT.Common.CompositionHelper;

namespace APTAT.Application.Services
{
    [Composition.CreationPolicy(Composition.CreationPolicy.Singleton)]
    public class WmiService : IWmiService
    {
        private readonly IDescentSubSystem _descentSubSystem;
        public WmiService(IDescentSubSystem descentSubSystem)
        {
            _descentSubSystem = descentSubSystem;
        }
        public void StartAnalyze(DeploymentTarget[] items, String login, String password, 
            DevicesSourceDescription devicesSourceDescription, Object userState)
        {
            _descentSubSystem.DescentCompleted += DescentSubSystemOnDescentCompleted;
            Guid operationUid;
            var scanner = new WmiScanner(items, login, password, devicesSourceDescription);
            try
            {
                _descentSubSystem.StartScan(scanner, out operationUid);
            }
            catch (Exception)
            {
                _descentSubSystem.DescentCompleted -= DescentSubSystemOnDescentCompleted;
                throw;
            }
            var scanOperationContext = (WmiScanOperationContext)_descentSubSystem.GetDescentResultsByOperationUid(operationUid);
            if (scanOperationContext != null)
            {
                scanOperationContext.OperationProgressChanged += ScanOperationContextOnOperationProgressChanged;
            }
        }

        private void DescentSubSystemOnDescentCompleted(object sender, DescentEventArgs e)
        {
            _descentSubSystem.DescentCompleted -= DescentSubSystemOnDescentCompleted;
            var scanOperationContext = (WmiScanOperationContext)_descentSubSystem.GetDescentResultsByOperationUid(e.OperationUid);
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
