using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Reports;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Valkyrie;

namespace APTAT.Core
{
    public interface IAptAtManager : IDisposable
    {
        event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<FileResult>> ValkyrieAnalyzeProgressChanged;
        event AnalyzeCompletedEventHandler<FileResult> ValkyrieAnalyzeCompleted;
        event EventHandler<FlsRequestEventArgs> FlsAnalyzeProgressChanged;
        event EventHandler ValkyrieAnalyzeStarted;

        IAptScanner GetAptScanner(DeploymentTargetsCollection targets, string login, string password, 
            IDevicesSourceDescription devicesSourceDescription, ScanOptions options);

        IAptReportBuilder GetAptReportBuilder();
      //  IAptScanResult GetScanResultByTargetUid(Guid targetUid);
        void StopScan();
        void UpdateFlsPorts(int udpPort, int tcpPort);

        void StartValkyrieAnalysis(string login, string password, 
            string camLicenseId, string camUserName, string camPassword, NetworkCredential proxyCredential);

        string GetValkyrieDetailsLink();
        int GetDistinctFileCount();

        
    }

    internal interface IAptAtManagerInternal : IAptAtManager
    {
        Guid ValkyrieOperationUid { get; }
        Guid FlsLookupOperationUid { get; }
        IAptScanOperationContext LastAptScanOperationContext { get; }
        ConcurrentDictionary<string, FileResult> ValkyrieAnalysisResult { get; }
    }
}