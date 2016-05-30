using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Core.Descent.Helpers;
using APTAT.Core.Descent.Transport;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core.Descent.Wmi
{
    class WmiScanner : Scanner
    {
        protected readonly ConcurrentDictionary<Guid, Thread> Threads = new ConcurrentDictionary<Guid, Thread>();
        public WmiScanner(ISubSystemCatalog catalog, IEnumerable<DeploymentTarget> targets, string login, string password,
            DevicesSourceDescription devicesSourceDescription)
            : base(catalog, targets, login, password, devicesSourceDescription)
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
                    co = GetConnectionOptionsForWmi(tp.RemoteHostInfo.Login, tp.RemoteHostInfo.Password);
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
                    co = GetConnectionOptionsForWmi(null, null);
                    ms = new ManagementScope(path, co);
                    ms.Connect();
                    
                }
                ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(ms, objectQuery);
                managementObjectSearcher.Options.Timeout = TimeSpan.FromMinutes(2);
                foreach (var mo in managementObjectSearcher.Get())
                {
                    var mainboardInfo = new MainboardInfo
                    {
                        Manufacturer = GetProperty(mo, "Manufacturer", ""),
                        ReleaseDate = GetProperty(mo, "ReleaseDate", ""),
                        SMBIOSBIOSVersion = GetProperty(mo, "SMBIOSBIOSVersion", "")
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
        private static ConnectionOptions GetConnectionOptionsForWmi(string login, string password)
        {
            ConnectionOptions connectionOptions = new ConnectionOptions();
            connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
            connectionOptions.EnablePrivileges = true;
            connectionOptions.Authentication = AuthenticationLevel.PacketPrivacy;
            if (login != "" && password != "")
            {
                connectionOptions.Username = login;
                connectionOptions.Password = password;
            }
            return connectionOptions;
        }
        private static string GetProperty(ManagementBaseObject mo, string propertyName, string defaultVal)
        {
            string str = "";
            try
            {
                if (mo[propertyName] is ushort[])
                {
                    ushort[] numArray = mo[propertyName] as ushort[];
                    if (numArray.Length > 0)
                    {
                        str = string.Concat(numArray[0]);
                        for (int index = 1; index < numArray.Length; ++index)
                            str = str + (object)", " + (string)(object)numArray[index];
                    }
                }
                else
                    str = !(mo[propertyName] is string[]) ? mo[propertyName].ToString() : string.Join(", ", mo[propertyName] as string[]);
            }
            catch
            {
                str = defaultVal;
            }
            return str.Trim();
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

        public override void StartScan(IDescentQueryResult result)
        {
            var scanResults = (WmiScanOperationContext) result;
            foreach (TargetUidComputerPair computer in Targets)
            {
                RemoteHostInfo ri = new RemoteHostInfo
                {
                    HostName = (computer.Computer.IPAddress != null) ? computer.Computer.IPAddress.ToString() : computer.Computer.DnsName,
                    Login = Login,
                    Password = Password,
                    TargetUid = computer.TargetUid
                };
                WmiScanResult scanResult;
                scanResults.ScanResults.TryGetValue(computer.TargetUid, out scanResult);
                var job = new RemoteInstallerJob(result, Tracker, computer.Computer);
                StartWmiScan(ri, job, computer.TargetUid, scanResult, result.OperationUid);
            }
        }
    }
}
