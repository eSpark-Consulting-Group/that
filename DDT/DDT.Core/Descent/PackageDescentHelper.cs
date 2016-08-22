using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent.Exceptions;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Descent.Helpers;
using APTAT.DeploymentCore.Descent.Helpers.Transport;

namespace DDT.Core.Descent
{
    public class PackageDescentHelper : DescentHelper
    {
        private const string DescentAgentName = "DescentHelper.exe";
        private string _responseFilePathForCurrentComputer;
        private const string DescentAgentResponseFileName = "DescentHelper.response";
        public EventHandler<InstallProgressChangedEventArgs> InstallProgressChanged;
        public EventHandler<SystemKindSpecifyEventArgs> SystemKindSpeciefied; 
        private readonly InstallPackageInfo _packageInfoX86;
        private readonly InstallPackageInfo _packageInfoX64;

        public PackageDescentHelper(RemoteHostInfo remoteHostInfo, InstallPackageInfo packageInfoX86
            , InstallPackageInfo packageInfoX64) 
            : base(remoteHostInfo)
        {
            if (packageInfoX86 == null) throw new ArgumentNullException("packageInfoX86 can not be null");
            _packageInfoX64 = packageInfoX64?? packageInfoX86;
            _packageInfoX86 = packageInfoX86;
        }

        protected override void InitPaths()
        {
            base.InitPaths();
            _responseFilePathForCurrentComputer = RemoteWorkingDirectory + @"\" + DescentAgentResponseFileName;

        }

        protected override string GetPackageFileName(bool is64)
        {
            return DescentAgentName;
        }

        protected override void CleanUpBeforeActionStarted()
        {
            base.CleanUpBeforeActionStarted();
            if (File.Exists(_responseFilePathForCurrentComputer))
                File.Delete(_responseFilePathForCurrentComputer);
        }

        public string GetRemotePath(string filePath)
        {
            if (System.IO.Directory.Exists(RemoteWorkingDirectory) == false)
                System.IO.Directory.CreateDirectory(RemoteWorkingDirectory);

            // Copy file to the target directory
            string targetPath = RemoteWorkingDirectory + @"\" + Path.GetFileName(filePath);
            return targetPath;
        }

        public bool Is64Bit()
        {
            return Directory.Exists(Path.Combine(RemoteAdminPath, "SysWOW64"));

        }
        protected override string[] GetAdditionalFilesToCopy(bool is64)
        {
            if (is64)
            {        
                return _packageInfoX64.GetPaths();
            }
            else
            {
                return  _packageInfoX86.GetPaths();              
                
            }            
        }
        public override void ExecuteAction(string actionName, string[] parameters, int timeoutInMs)
        {
            List<string> args = new List<string>();
            args.Add(DescentAgentName); // file name
            args.Add("process_response_action"); // command
            args.Add(actionName); // action name
            args.Add(_responseFilePathForCurrentComputer); // response file name
            args.Add(timeoutInMs.ToString()); // timeout in ms

            foreach (string param in parameters)
            {
                args.Add(param);
            }
            base.ExecuteAction(actionName, args.ToArray(), timeoutInMs);
            // Read the response file contents
            int attemptsCount = 30;
            int checkFileHeaderAttemptsCount = 20;
            int readFileAttemptsCount = 20;
            int checkAgentRunning = 20;
            int? percentage = null;
            SystemKind? systemKind = null;
            while (attemptsCount > 0)
            {
                --attemptsCount;
                try
                {
                    string[] lines = new string[4];
                    bool isServiceRunning = ServiceHelper.IsServiceRunning();
                    using (TextReader sr = new StreamReader(_responseFilePathForCurrentComputer,
                        System.Text.Encoding.Unicode))
                    {
                        attemptsCount = 20;
                        lines[0] = sr.ReadLine();
                        if (lines[0] == null)
                        {
                            if (readFileAttemptsCount > 0)
                                --readFileAttemptsCount;
                            throw new StatusFileEmptyException();
                        }
                        readFileAttemptsCount = 20;
                        for (int i = 1; i < lines.Length; i++)
                            lines[i] = sr.ReadLine();
                        _response.Response += lines[1];
                        _response.Response = "";
                        string line = sr.ReadLine();
                        while ((line = sr.ReadLine()) != null)
                        {
                            _response.Response += line;
                        }
                        sr.Close();
                    }
                    string[] splitted = lines[0].Split(';', '#');
                    if (splitted.Length != 3)
                    {
                        if (checkFileHeaderAttemptsCount > 0)
                            --checkFileHeaderAttemptsCount;
                        throw new ResponseFileHeaderInvalidException();
                    }
                    checkFileHeaderAttemptsCount = 20;
                    _response.Status = (DescentStatus) Int32.Parse(splitted[0]);
                    _response.Win32Error = Int32.Parse(splitted[1]);
                    _response.ErrorString = splitted[2];
                    if (systemKind == null)
                    {
                        systemKind = (SystemKind) UInt32.Parse(lines[3]);
                        InvokeSystemKindSpecified(RemoteHostInfo.TargetUid, systemKind.Value);
                    }

                    if (_response.Status == DescentStatus.status_install_in_progress
                        || Response.Status == DescentStatus.status_prepairing_for_create_process
                        || Response.Status == DescentStatus.status_before_msi_started)
                    {
                        if (!isServiceRunning && checkAgentRunning-- ==0)
                        {
                            throw new DescentException(DescentStatus.status_agent_was_crashed, "Agent was crashed.", 0);
                        }
                        else if (isServiceRunning)
                            checkAgentRunning = 20;
                        string[] splittedProgress = lines[2]?.Split(';', '#');
                        if (splittedProgress != null && splittedProgress.Length != 2)
                        {
                            throw new ResponseFileHeaderInvalidException();
                        }

                        var currentPercentage = Int32.Parse(splittedProgress[0]);


                        if (percentage == null || currentPercentage != percentage.Value)
                        {
                            percentage = currentPercentage;
                            InvokeInstallProgressChanged(RemoteHostInfo.TargetUid, percentage.Value, actionName);
                        }
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }
                }
                catch (IOException ex)
                {
                    // If file is not found, we can sleep and try again
                    RaiseExceptionOrRetry(attemptsCount, ex.Message);
                }
                catch (ResponseFileHeaderInvalidException ex)
                {
                    // If file is not found, we can sleep and try again
                    RaiseExceptionOrRetry(checkFileHeaderAttemptsCount, ex.Message);
                }
                catch (StatusFileEmptyException ex)
                {
                    // If file is not found, we can sleep and try again
                    RaiseExceptionOrRetry(readFileAttemptsCount, ex.Message);
                }
                catch (OperationCanceledException)
                {
                    throw new ScanCanceledException();
                }
                catch (ThreadAbortException ex)
                {
                    if ((ex.ExceptionState is bool) == true)
                    {
                        throw new ScanCanceledException();
                    }
                }
                catch (DescentException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    RaiseExceptionOrRetry(0, ex.Message);
                }
            }
        }

        private void RaiseExceptionOrRetry(int checkFileHeaderAttemptsCount, string exceptionMessage)
        {
            if (checkFileHeaderAttemptsCount == 0)
            {
                try
                {
                    bool isServiceRunning = ServiceHelper.IsServiceRunning();
                }
                catch (Exception ex)
                {
                    throw new DescentException(DescentStatus.status_remote_invalid_response,
                    ex.Message, 0);
                }
                // unless attempts count is zero...
                throw new DescentException(DescentStatus.status_remote_invalid_response,
                    exceptionMessage, 0);
            }
            else
            {
                // sleep and try again
                Thread.Sleep(1000);
            }
        }

        private void InvokeInstallProgressChanged(Guid targetUid, int percentage, string actionName)
        {
            EventHandler<InstallProgressChangedEventArgs> temp = InstallProgressChanged;
            if (temp != null)
                temp(this, new InstallProgressChangedEventArgs(targetUid, percentage, actionName));
        }

        private void InvokeSystemKindSpecified(Guid targetUid, SystemKind systemKind)
        {
            var temp = SystemKindSpeciefied;
            if (temp != null)
                temp(this, new SystemKindSpecifyEventArgs(targetUid, systemKind));
        }

        public class SystemKindSpecifyEventArgs : DescentHelperEventArgs
        {
            public SystemKind SystemKind { get; private set; }
            public SystemKindSpecifyEventArgs(Guid targetUid, SystemKind systemKind) 
                : base(targetUid)
            {
                SystemKind = systemKind;
            }
        }

        public class InstallProgressChangedEventArgs : DescentHelperEventArgs
        {
            public int Percentage { get; private set; }
            public string ActionName { get; private set; }
            public InstallProgressChangedEventArgs(Guid targetUid, int percentage, string actionName) 
                : base(targetUid)
            {
                Percentage = percentage;
                ActionName = actionName;
            }
        }
    }
}
