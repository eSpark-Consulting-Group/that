using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Descent.Helpers;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Common.MachineKey;
using APTAT.DeploymentCore;

namespace APTAT.Core.Descent.Wmi
{
    public class WmiScanner : Scanner
    {
        protected readonly ConcurrentDictionary<Guid, Thread> Threads = new ConcurrentDictionary<Guid, Thread>();
        public WmiScanner(IEnumerable<IComputer> targets, string login, string password,
            DevicesSourceDescription devicesSourceDescription)
            : base(targets, login, password, devicesSourceDescription)
        {

        }
        public void StartWmiScan(RemoteHostInfo remoteHostInfo, IDescentCallback cb, Guid agentIdentity,
            WmiScanResult result, Guid operationUid, string args = null)
        {
            WmiScannerThreadParams tp = new WmiScannerThreadParams();
            tp.RemoteHostInfo = remoteHostInfo;
            tp.DescentCallback = cb;
            tp.AgentIdentity = agentIdentity;
            tp.OperationUid = operationUid;
            tp.Result = result;
            tp.args = args;

            //Thread thread = new Thread(InstallAgentThread);
            //thread.Start(tp);
            TaskFactory.StartNew(StartWmiScanThread, tp, TaskCreationOptions.LongRunning).ContinueWith(task => task.Dispose());
            //ThreadPool.QueueUserWorkItem(InstallAgentThread_, tp);
        }

        private void StartWmiScanThread(object obj)
        {
            var tp = (WmiScannerThreadParams)obj;
            tp.DescentCallback.SetDescentThread();
            try
            {
                ConnectionOptions co;
                ManagementScope ms = null;
                ObjectQuery objectQuery = new SelectQuery("Win32_Bios");
                bool tryWithoutCred = false;
                var path = String.Format("\\\\{0}\\root\\CIMV2", tp.RemoteHostInfo.HostName);
                try
                {
                    co = WmiHelper.GetConnectionOptionsForWmi(tp.RemoteHostInfo.Login, tp.RemoteHostInfo.Password);
                    ms = new ManagementScope(path, co);
                    ms.Connect();
                }
                catch (Exception ex)
                {
                    if (ex.Message.ToLower().Contains("for local connections"))
                        tryWithoutCred = true;
                    else
                        throw;
                }
                if (tryWithoutCred)
                {
                    co = WmiHelper.GetConnectionOptionsForWmi(null, null);
                    ms = new ManagementScope(path, co);
                    ms.Connect();
                    
                }
                ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(ms, objectQuery);
                managementObjectSearcher.Options.Timeout = TimeSpan.FromMinutes(2);
                foreach (var mo in managementObjectSearcher.Get())
                {
                    var mainboardInfo = new MainboardInfo
                    {
                        Manufacturer = WmiHelper.GetProperty(mo, "Manufacturer", ""),
                        ReleaseDate = WmiHelper.GetProperty(mo, "ReleaseDate", ""),
                        SMBIOSBIOSVersion = WmiHelper.GetProperty(mo, "SMBIOSBIOSVersion", "")
                    };
                    tp.Result.MainboardInfo = mainboardInfo;
                }
                tp.DescentCallback.OnDescentCallback(DescentStatus.status_success, 0, String.Empty);
            }
            catch (ThreadAbortException ex)
            {
                if ((ex.ExceptionState as string) == TimeoutAbortString)
                {
                    LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent, "Descent operation timed out for computer {0}. " +
                        "Thread terminated with exception: ", tp.RemoteHostInfo.HostName);
                    Thread.ResetAbort();
                }

                if (ex.ExceptionState is bool)
                {
                    LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent, "Scan was canceled for '{0}' computer.", 
                        tp.RemoteHostInfo.HostName);
                    ProcessErrorOrCancel(DescentStatus.status_operation_canceled, String.Empty, tp);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Trace(TraceEventType.Error, TraceCategory.Descent, "{0} occurred in wmi thread: {1}", ex.GetType().Name, ex.Message);
                ProcessErrorOrCancel(DescentStatus.status_error, ex.Message, tp);
            }
        }
        private void ProcessErrorOrCancel(DescentStatus descentStatus, string errorMessage, WmiScannerThreadParams tp)
        {
            WmiScanResult result = tp.Result;
            if (result != null)
            {
                string errorText = errorMessage;
                ScanResultType scanResultType = ScanResultType.Failed;
                if (descentStatus == DescentStatus.status_operation_canceled)
                    scanResultType = ScanResultType.Canceled;

                result.ResultErrorText = errorText;
                result.ResultType = scanResultType;
            }

            if (tp.DescentCallback != null)
                tp.DescentCallback.OnDescentCallback(descentStatus, 1, errorMessage);
        }
        

        private struct WmiScannerThreadParams
        {
            public RemoteHostInfo RemoteHostInfo;
            public IDescentCallback DescentCallback;
            public WmiScanResult Result;
            public Guid AgentIdentity;
            public Guid OperationUid;
            public string args;
        }

        public override IDescentQueryResult ValidateAndGetResult()
        {
            var result = new WmiScanOperationContext(Targets, DevicesSourceDescription);
            return result;
        }

        public override void StartScan(IChangeTrackingSubSystem changeTrackingSubSystem,IDescentQueryResult result)
        {
            var scanResults = (WmiScanOperationContext) result;
            foreach (var computer in Targets)
            {
                RemoteHostInfo ri = new RemoteHostInfo(computer.Uid,
                    (computer.IPAddress != null) ? computer.IPAddress.ToString() : computer.DnsName,
                    Login, Password, computer.IsLocalComputer);
                WmiScanResult scanResult;
                scanResults.ScanResults.TryGetValue(computer.Uid, out scanResult);
                var job = new RemoteInstallerJob(result, changeTrackingSubSystem, this, computer);
                StartWmiScan(ri, job, computer.Uid, scanResult, result.OperationUid);
            }
        }

        public override void StopScan(Guid targetUid)
        {
            throw new NotImplementedException();
        }
    }
}
