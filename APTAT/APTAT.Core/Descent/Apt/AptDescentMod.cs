using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.Core.Descent.Apt.Transport;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Descent.Helpers;
using APTAT.DeploymentCore.Descent.Helpers.Transport;
using APTAT.DeploymentCore.Messages;
using APTAT.DeploymentCore.Properties;

namespace APTAT.Core.Descent.Apt
{
    public class AptDescentMod : DescentMod
    {
        private readonly ConcurrentDictionary<Guid, AptInstallAgentThreadParams> _paramsPerTargetUid 
            = new ConcurrentDictionary<Guid,AptInstallAgentThreadParams>(); 
        public AptDescentMod(TaskFactory taskFactory, IChangeTrackingSubSystem changeTrackingSubSystem)
            : base (taskFactory, changeTrackingSubSystem)
		{
		}

        protected class AptInstallAgentThreadParams : InstallAgentThreadParams
        {
            public AptScanResult Result { get; set; }
         
            public ScanOptions Options { get; set; }
        }
        public void StartAptScan(RemoteHostInfo remoteHostInfo, IDescentCallback cb, Guid agentIdentity,
            AptScanResult result, Guid operationUid,  ScanOptions scanOptions)
        {
            AptInstallAgentThreadParams tp = new AptInstallAgentThreadParams
            {
                RemoteHostInfo = remoteHostInfo,
                DescentCallback = cb,
                AgentIdentity = agentIdentity,
                OperationUid = operationUid,
                Result = result,
         
                Options = scanOptions
            };
            _paramsPerTargetUid[agentIdentity] = tp;
            StartDeploymentThread(tp);
        }

        protected virtual void OnEnumerationProgressChanged(object sender, AptDescentHelper.AptEnumeratingEventArgs args)
        {
            bool notify = true;
            string fileTypes = String.Empty;
            AptScanStage scanStage = AptScanStage.Pending;
            switch (args.Stage)
            {
                case EnAptStage.EnApt_EnumeratingProcesses:
                    fileTypes = "processes";
                    scanStage = AptScanStage.EnumeratingProcesses;
                    break;
                case EnAptStage.EnApt_EnumeratingDrivers:
                    fileTypes = "drivers";
                    scanStage = AptScanStage.EnumeratingDrivers;
                    break;
                case EnAptStage.EnApt_EnumeratingAutoruns:
                    fileTypes = "autoruns";
                    scanStage = AptScanStage.EnumeratingAutoruns;
                    break;
                case EnAptStage.EnApt_EnumeratingFolder:
                    fileTypes = "folders";
                    scanStage = AptScanStage.EnumeratingFolders;
                    break;
                default:
                    notify = false;
                    break;
            }

            if (notify)
            {
                var text = new ResourceText("null", $"Enumerating {fileTypes}: {args.EnumeratedFilesCount}");
                var tp = _paramsPerTargetUid[args.TargetUid];
                NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text, scanStage, 0);
            }
        }

        protected virtual void OnScanCompleted(object sender, AptDescentHelper.ScanCompletedEventArgs args)
        {
            var tp = _paramsPerTargetUid[args.TargetUid];
            string errorCounterText = (args.FailedToAnalyzeFilesCount+args.NoVerdictFilesCount) > 0 ?
                $"Lookup in Progress: {args.FailedToAnalyzeFilesCount}."
                : String.Empty;

            ResourceText text = new ResourceText("null", string.Format(format: "{0} files scanned ({3}%). Unknown files: {1}. Malicious files: {2}. {4} {5}", args: new object[] { args.TotalFilesScanned, args.UnknownFilesFound, args.MalwaresFound,
                $"{args.Percentage:0.00}".Replace(".00", string.Empty), errorCounterText, "" }));

            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text, AptScanStage.ScanCompleted, args.TotalFilesScanned, args.UnknownFilesFound, args.MalwaresFound, args.SafeFilesFound, 100, args.FailedToAnalyzeFilesCount, args.NoVerdictFilesCount);

        }

        protected virtual void OnScanFailed(object sender, AptDescentHelper.ScanFailedEventArgs args)
        {
            var tp = _paramsPerTargetUid[args.TargetUid];
            var text = new ResourceText("null", $"Scan failed: {args.Status}.");
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text, AptScanStage.ScanFailed, args.TotalFilesScanned, args.UnknownFilesFound, args.MalwaresFound, args.SafeFilesFound, 100, args.FailedToAnalyzeFilesCount, args.NoVerdictFilesCount);

        }

        protected virtual void OnScanResultsFileReceived(object sender, AptDescentHelper.ScanResultsFileReceivedEventArgs args)
        {
            var tp = _paramsPerTargetUid[args.TargetUid];
            AptScanResult result = tp.Result;

            if (result != null)
            {
                result.FileName = args.FileName;
                result.TotalFilesCount = args.TotalFilesScanned;
                result.UnknownFilesCount = args.UnknownFilesFound;
                result.SafeFilesCount = args.SafeFilesFound;
                result.MalwareFilesCount = args.MalwaresFound;
                result.EmptyFilesCount = args.EmptyFilesFound;
                result.SkippedFilesCount = args.SkippedFilesCount;
                result.FailedToAnalyzeFilesCount = args.FailedToAnalyzeFilesCount;
                result.NoVerdictFilesCount = args.NoVerdictFilesCount;
            }
            //_scanResults.TryAdd(args.TargetUid, new ScanResult(){TargetUid = args.TargetUid, HostName = args.HostName, FileName = args.FileName, TotalFilesCount = args.TotalFilesScanned, UnknownFilesCount = args.UnknownFilesFound});

        }

        protected virtual void OnResultsReceiving(object sender, DescentHelperEventArgs args)
        {
            var text = new ResourceText("null", "Obtaining results... ");
            var tp = _paramsPerTargetUid[args.TargetUid];
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text,
                AptScanStage.ScanResultsObtaning, 100);
        }
        protected override DescentHelper CreateDescentHelper(InstallAgentThreadParams agentTp)
        {
            var tp = (AptInstallAgentThreadParams)agentTp;
            var dh = new AptDescentHelper(tp.RemoteHostInfo, tp.Options);
            
            dh.ScanProgressChanged += OnScanProgressChanged;
            dh.ScanCompleted += OnScanCompleted;
            dh.ScanFailed += OnScanFailed;
            dh.ScanResultsFileReceived += OnScanResultsFileReceived;
            dh.ReceivingResults += OnResultsReceiving;
            dh.EnumerationProgressChanged += OnEnumerationProgressChanged;
            return dh;
        }

        protected override void DeploymentThread(object obj)
        {
            var tp = (AptInstallAgentThreadParams)obj;
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity,
                        DeploymentCore.Properties.ServerSideMessagesResources.DescentProgressAgentInstallConnectingToTarget,
                        AptScanStage.Connecting, 0);
            base.DeploymentThread(obj);
        }

        protected override void ExecuteAction(DescentHelper dh, InstallAgentThreadParams tp)
        {
            base.ExecuteAction(dh, tp);
            dh.ExecuteAction(String.Empty, new string[0], 0);
        }

        protected override void ProcessResults(InstallAgentThreadParams agentTp, DescentHelper dh)
        {
            base.ProcessResults(agentTp, dh);
            var tp = (AptInstallAgentThreadParams)agentTp;
            var resultsProcessor = new ScanResultsProcessor();
            AptScanResult result = tp.Result;
            if (result != null)
            {
                var text = new ResourceText("null",
                    $"Scan results received. Total files scanned: {result.TotalFilesCount}. " +
                    $"Unknown files: {result.UnknownFilesCount}. Malicious files: {result.MalwareFilesCount}. Lookup in Progress: {result.LookupInProgressFilesCount}");
                NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text,
                    AptScanStage.ScanResultsProcessing, result.TotalFilesCount, result.UnknownFilesCount,
                    result.MalwareFilesCount, result.SafeFilesCount, 100, result.FailedToAnalyzeFilesCount,
                    result.NoVerdictFilesCount);

                if (!resultsProcessor.TryParse(ref result))
                    throw new DescentException(DescentStatus.status_error, "Cannot process scan results.", 0);

                result.ResultType = ScanResultType.Completed;
                NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity,
                    new ResourceText("null",
                        String.Format(
                            "Completed: Total files scanned: {0}. Unknown files: {1}. Malicious files: {2}.",
                            result.TotalFilesCount, result.UnknownFilesCount, result.MalwareFilesCount) +
                            (result.LookupInProgressFilesCount > 0 ? String.Format(" Lookup in Progress: {0}.", result.LookupInProgressFilesCount) : String.Empty)),
                    AptScanStage.RemoteOperationCompleted, result.TotalFilesCount, result.UnknownFilesCount,
                    result.MalwareFilesCount, result.SafeFilesCount, 100, result.FailedToAnalyzeFilesCount,
                    result.NoVerdictFilesCount);
            }
        }

        private void NotifyProgressChanged(Guid operationUid, string hostName, Guid computerUid, ResourceText resourceMessage, AptScanStage scanStage, double percentage)
        {
            NotifyProgressChanged(operationUid, hostName, computerUid, resourceMessage, scanStage, 0, 0, 0, 0, percentage, 0, 0);
        }

        private void NotifyProgressChanged(Guid operationUid, string hostName, Guid computerUid, ResourceText resourceMessage, AptScanStage scanStage,
            int totalFilesCount, int unknownFilesCount, int malwaresCount, int safeFilesCount, double percentage, int errorFilesCount, int failedAtNodeCount)
        {
            if ((percentage == 0 || double.IsInfinity(percentage)) && scanStage == AptScanStage.ScanInProgress)
                return;

            var tp = _paramsPerTargetUid[computerUid];
            if (TaskFactory.CancellationToken.IsCancellationRequested || tp.IsCancelled) 
                throw new OperationCanceledException();
            FormattedText formattedText = new FormattedText(resourceMessage);
            Tracker?.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed, 
                new AptDescentProgressItem(computerUid, hostName, operationUid, formattedText, scanStage, totalFilesCount, unknownFilesCount
                , malwaresCount, safeFilesCount, percentage, errorFilesCount, failedAtNodeCount)), false);
        }

        protected override void OnDescentHelperCopied(object sender, DescentHelperEventArgs args)
        {
            var tp = _paramsPerTargetUid[args.TargetUid];
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity,
                ServerSideMessagesResources.DescentProgressAgentInstallConfiguringHelperService, AptScanStage.ConfiguringScanner, 0);
        }

        protected override void OnDescentHelperParametersCopied(object sender, DescentHelperEventArgs args)
        {
            var tp = _paramsPerTargetUid[args.TargetUid];
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity,
                ServerSideMessagesResources.DescentProgressAgentInstallConfiguringHelperService, AptScanStage.ConfiguringScanner, 0);
        }

        protected override void OnDescentHelperStarted(object sender, DescentHelperEventArgs args)
        {
        }

        protected override void OnRemotelyConnected(object sender, DescentHelperEventArgs args)
        {
            var tp = _paramsPerTargetUid[args.TargetUid];
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName,
                    tp.AgentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallCopyingHelperServicePackage, 
                    AptScanStage.CopyingScanner, 0);
        }

        protected virtual void OnScanProgressChanged(object sender, AptDescentHelper.ScanProgressChangedEventArgs args)
        {
            var lookupInProgressCount = (args.FailedToAnalyzeFilesCount + args.NoVerdictFilesCount);
            string errorCounterText = lookupInProgressCount > 0 ? $"Lookup in Progress: {lookupInProgressCount}."
                : String.Empty;
            //string errorCounterText = args.FailedToAnalyzeFilesCount > 0 ? String.Format("Failed to analyze: {0}.", args.FailedToAnalyzeFilesCount) : String.Empty;

            //string serverErrorCounterText = args.NoVerdictFilesCount > 0 ? String.Format("No Verdict: {0}.", args.NoVerdictFilesCount) : String.Empty;

            ResourceText text = new ResourceText("null",
                $"{args.TotalFilesScanned} files scanned ({$"{args.Percentage:0.00}".Replace(".00", string.Empty)}%). Unknown files: {args.UnknownFilesFound}. Malicious files: {args.MalwaresFound}. {errorCounterText} ");

            var tp = _paramsPerTargetUid[args.TargetUid];
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text, AptScanStage.ScanInProgress, args.TotalFilesScanned, args.UnknownFilesFound, args.MalwaresFound, args.SafeFilesFound, args.Percentage, args.FailedToAnalyzeFilesCount, args.NoVerdictFilesCount);

        }

        protected override void DescentHelperBeforeRelease(DescentHelper dh)
        {
            base.DescentHelperBeforeRelease(dh);
            var adh = (AptDescentHelper) dh;
            adh.ScanProgressChanged -= OnScanProgressChanged;
            adh.ScanCompleted -= OnScanCompleted;
            adh.ScanFailed -= OnScanFailed;
            adh.ScanResultsFileReceived -= OnScanResultsFileReceived;
            adh.ReceivingResults -= OnResultsReceiving;
            adh.EnumerationProgressChanged -= OnEnumerationProgressChanged;
        }

        protected override void ProcessErrorOrCancel(DescentStatus descentStatus, int win32ErrorCode, string errorMessage, 
            InstallAgentThreadParams atp)
        {
            var tp = (AptInstallAgentThreadParams)atp;
            AptScanResult result = tp.Result;
            if (result != null)
            {
                string errorText = errorMessage;
                ScanResultType scanResultType = ScanResultType.Failed;
                if (descentStatus == DescentStatus.status_operation_canceled)
                    scanResultType = ScanResultType.Canceled;

                result.ResultErrorText = errorText;
                result.ResultType = scanResultType;
            }
            base.ProcessErrorOrCancel(descentStatus, win32ErrorCode, errorMessage, tp);
        }
    }
}