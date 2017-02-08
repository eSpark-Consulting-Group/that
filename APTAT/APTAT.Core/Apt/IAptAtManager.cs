using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Reports;
using APTAT.DeploymentCore.Directory;
using APTAT.Valkyrie;
using System.Collections.ObjectModel;
using APTAT.Core.Apt;
using APTAT.Core.FLS;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core
{
    public interface IAptAtManager : IDisposable
    {
        event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<FileResult>> ValkyrieAnalyzeProgressChanged;
        event AnalyzeCompletedEventHandler<FileResult> ValkyrieAnalyzeCompleted;
        event EventHandler<FlsRequestEventArgs<IAptVerdictContext>> FlsAnalyzeProgressChanged;
        event EventHandler ValkyrieAnalyzeStarted;
        IAggregatedAptScanResult AggregatedScanResults { get; }

        IAptScanner GetAptScanner(DeploymentTarget[] targets, IDevicesSourceDescription devicesSourceDescription, ScanOptions options);

        IAptReportBuilder GetAptReportBuilder();
        int GetDistinctFilesCount();
        IAptVerdictContext[] GetModifiedFileVerdicts(DateTime? modifiedDate);
        FileResult[] GetModifiedValkyrieVerdicts(DateTime? modifiedDate, Dictionary<string, IModule> modules
            , out Dictionary<string, ValkyrieAnalysisResult> valkyrieAnalysisResults);

        void StopScan(Guid[] targets);
        void UpdateFlsPorts(int udpPort, int tcpPort);

        void StartValkyrieAnalysis(string login, string password, 
            string camLicenseId, string camUserName, string camPassword, NetworkCredential proxyCredential, string productName);

        string GetValkyrieDetailsLink();
    }

    internal interface IAptAtManagerInternal : IAptAtManager
    {
        Guid ValkyrieOperationUid { get; }
        Guid FlsLookupOperationUid { get; }
        Dictionary<string, FileResult> ValkyrieAnalysisResult { get; }
    }
}