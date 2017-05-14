using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent.Exceptions;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Descent.Helpers;
using APTAT.DeploymentCore.Descent.Helpers.Transport;

namespace APTAT.Core.Descent.Copy
{
    public class CopyDescentHelper : DescentHelper
    {
        private const string DescentAgentName = "DescentCopyHelper.exe";
        private string _responseFilePathForCurrentComputer;
        private string _filePathWithFilesForCurrentComputer;
        private const string DescentAgentResponseFileName = "DescentCopyHelper.response";
        private const string FilePathWithFilesForCurrentComputer = "files";
        public EventHandler<CopyProgressChangedEventArgs> CopyProgressChanged;
        public EventHandler<CopyCompletedEventArgs> CopyCompleted;
        private readonly CancellationToken _cancellationToken;
        private readonly string[] _filePathes;

        public CopyDescentHelper(RemoteHostInfo remoteHostInfo, string[] filePathes, CancellationToken cancellationToken)
            : base(remoteHostInfo, cancellationToken)
        {
            if (filePathes == null) throw new ArgumentNullException("filePathes can not be null");
            _filePathes = filePathes;
            _cancellationToken = cancellationToken;
        }

        protected override void InitPaths()
        {
            base.InitPaths();
            _responseFilePathForCurrentComputer = RemoteWorkingDirectory + @"\" + DescentAgentResponseFileName;
            _filePathWithFilesForCurrentComputer = RemoteWorkingDirectory + @"\" + FilePathWithFilesForCurrentComputer;
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
            if (File.Exists(_filePathWithFilesForCurrentComputer))
                File.Delete(_filePathWithFilesForCurrentComputer);

            File.WriteAllText(_filePathWithFilesForCurrentComputer, String.Join(";", _filePathes));
        }

        protected override string[] GetAdditionalFilesToCopy(bool is64)
        {
            return base.GetAdditionalFilesToCopy(is64);
        }

        protected override string[] GetServiceParams()
        {
            return base.GetServiceParams();
        }

        public override void ExecuteAction(string actionName, string[] parameters, int timeoutInMs)
        {
            List<string> args = new List<string>();
            args.Add(_responseFilePathForCurrentComputer); // response file name
            args.Add(_filePathWithFilesForCurrentComputer); // files with file lists


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
            int lineCount = 0;
            while (attemptsCount > 0)
            {
                --attemptsCount;
                try
                {
                    List<string> lines = new List<string>(_filePathes.Length);
                    string status;
                    bool? isServiceRunning = ServiceHelper.IsServiceRunning();
                    using (TextReader sr = new StreamReader(_responseFilePathForCurrentComputer,
                        System.Text.Encoding.Unicode))
                    {
                        attemptsCount = 20;
                        status = sr.ReadLine();
                        if (status == null)
                        {
                            if (readFileAttemptsCount > 0)
                                --readFileAttemptsCount;
                            throw new StatusFileEmptyException();
                        }
                        readFileAttemptsCount = 20;

                        string line;

                        while ((line = sr.ReadLine()) != null)
                        {
                            lines.Add(line);
                        }
                        sr.Close();
                    }
                    string[] splitted = status.Split(';', '#');
                    if (splitted.Length != 3)
                    {
                        if (checkFileHeaderAttemptsCount > 0)
                            --checkFileHeaderAttemptsCount;
                        throw new ResponseFileHeaderInvalidException();
                    }
                    checkFileHeaderAttemptsCount = 20;
                    _response.Status = (DescentStatus)Int32.Parse(splitted[0]);
                    _response.Win32Error = Int32.Parse(splitted[1]);
                    _response.ErrorString = splitted[2];


                    if (_response.Status == DescentStatus.status_install_in_progress
                        || Response.Status == DescentStatus.status_prepairing_for_create_process
                        || Response.Status == DescentStatus.status_before_msi_started)
                    {
                        if (_cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }
                        if (isServiceRunning.HasValue && !isServiceRunning.Value && checkAgentRunning-- == 0)
                        {
                            throw new DescentException(DescentStatus.status_agent_was_crashed, "Agent was crashed.", 0);
                        }
                        else if (checkAgentRunning >= 0)
                            checkAgentRunning = 20;
                        if (lineCount != lines.Count)
                            InvokeCopyProgressChanged(lines);
                        Thread.Sleep(1000);
                    }
                    else if (_response.Status == DescentStatus.status_success)
                    {
                        InvokeCopyCompleted(lines);
                        break;
                    }
                    else
                        break;
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
                    var isServiceRunning = ServiceHelper.IsServiceRunning();
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

        private void InvokeCopyProgressChanged(IEnumerable<string> filePathes)
        {
            EventHandler<CopyProgressChangedEventArgs> temp = CopyProgressChanged;
            if (temp != null)
                temp(this, new CopyProgressChangedEventArgs(RemoteHostInfo.TargetUid, 
                    filePathes.Count() / _filePathes.Length, filePathes.ToArray()));
        }

        private void InvokeCopyCompleted(IEnumerable<string> filePathes)
        {
            EventHandler<CopyCompletedEventArgs> temp = CopyCompleted;
            if (temp != null)
                temp(this, new CopyCompletedEventArgs(RemoteHostInfo.TargetUid,
                    filePathes.ToArray()));
        }

        public class CopyCompletedEventArgs : DescentHelperEventArgs
        {
            public string[] FilePathes { get; }
            public CopyCompletedEventArgs(Guid targetUid, string[] filePathes)
                : base(targetUid)
            {
                FilePathes = filePathes;
            }
        }

        public class CopyProgressChangedEventArgs : DescentHelperEventArgs
        {
            public int Percentage { get; }
            public string[] FilePathes { get; }
            public CopyProgressChangedEventArgs(Guid targetUid, int percentage, string[] filePathes)
                : base(targetUid)
            {
                Percentage = percentage;
                FilePathes = filePathes;
            }
        }
    }
}
