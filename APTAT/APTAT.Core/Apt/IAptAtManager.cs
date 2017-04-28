using System;
using System.Collections.Generic;
using System.Net;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Reports;
using APTAT.DeploymentCore.Directory;
using APTAT.Core.Valkyrie;
using System.Threading.Tasks;
using APTAT.Core.Apt;
using APTAT.Core.FLS;
using APTAT.DeploymentCore;

namespace APTAT.Core
{
    public interface IAptAtManager : IDisposable
    {
        event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<FileResult>> ValkyrieAnalyzeProgressChanged;
        event EventHandler<AnalyzeCompletedEventArgs> ValkyrieAnalyzeCompleted;
        event EventHandler<FlsRequestEventArgs<IAptVerdictContext>> FlsAnalyzeProgressChanged;
        event EventHandler ValkyrieAnalyzeStarted;
        IAggregatedAptScanResult AggregatedScanResults { get; }

        IAptScanner GetAptScanner(IComputer[] targets, IDevicesSourceDescription devicesSourceDescription, IScanOptions options);
                
        int GetDistinctFilesCount();
        IAptVerdictContext[] GetModifiedFileVerdicts(DateTime? modifiedDate);
        IValkyrieTrackableInfo GetModifiedValkyrieVerdicts(DateTime? modifiedDate, Dictionary<string, IModule> modules
            , out Dictionary<string, IValkyrieAnalysisResult> valkyrieAnalysisResults);

        void StopScan(Guid[] targets);
        Task OnAppClosing();
        void UpdateFlsPorts(int udpPort, int tcpPort);

        void StartValkyrieAnalysis(string login, string password, 
            string camLicenseId, string camUserName, string camPassword, NetworkCredential proxyCredential, string productName);

        string GetValkyrieDetailsLink();
        string GetValkyrieDetailsLink(string apiKey, string scanId);

        IOperationResult SubmitEmail(string[] addresses);
        Task<ReportFile> BuildReportAsync(ReportType reportType, bool saveFile);        
    }    
}