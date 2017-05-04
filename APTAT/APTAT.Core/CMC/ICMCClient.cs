using System.Net;
using APTAT.Core.CMC.Request;

namespace APTAT.Core.CMC
{
    public interface ICmcClient
    {
        void ApplicationLaunched(IApplicationLaunchedRequest request);
        void AutoDiscoveryFinished(IAutoDiscoveryFinishedRequest request);
        void ScanStarted(IScanStartedRequest request);
        void ScanFinished(IScanFinishedRequest request);
        void ScanUpdated(IScanUpdatedRequest request);
        void ReportGenerated(IReportGeneratedRequest request);
        void ReportPrintedOrSaved(IReportPrintedOrSavedRequest request);
        void ApplicationClosed(IApplicationClosedRequest request);
    }

    
}
