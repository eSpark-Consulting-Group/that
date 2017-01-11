using System;
using System.Collections.Concurrent;
using System.Net;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Reports;
using APTAT.DeploymentCore.Directory;
using APTAT.Valkyrie;
using System.Collections.ObjectModel;

namespace APTAT.Core
{
    public interface IAptAtManager : IDisposable
    {
        event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<FileResult>> ValkyrieAnalyzeProgressChanged;
        event AnalyzeCompletedEventHandler<FileResult> ValkyrieAnalyzeCompleted;
        event EventHandler<FlsRequestEventArgs> FlsAnalyzeProgressChanged;
        event EventHandler ValkyrieAnalyzeStarted;

        IAptScanner GetAptScanner(IDevicesSourceDescription devicesSourceDescription, ScanOptions options);

        IAptReportBuilder GetAptReportBuilder();

        void StopScan(Guid[] targets);
        void UpdateFlsPorts(int udpPort, int tcpPort);

        void StartValkyrieAnalysis(string login, string password, 
            string camLicenseId, string camUserName, string camPassword, NetworkCredential proxyCredential);

        string GetValkyrieDetailsLink();
        int GetDistinctFilesCount();



    }

    internal interface IAptAtManagerInternal : IAptAtManager
    {
        Guid ValkyrieOperationUid { get; }
        Guid FlsLookupOperationUid { get; }
        Collection<IAptScanOperationContext> AptScanOperationContexts { get; }
        ConcurrentDictionary<string, FileResult> ValkyrieAnalysisResult { get; }
    }
}