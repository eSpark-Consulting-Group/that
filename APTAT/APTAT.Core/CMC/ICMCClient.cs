using System.Net;
using APTAT.Core.CMC.Request;

namespace APTAT.Core.CMC
{
    public interface ICmcClient
    {
        void ApplicationLaunched(IApplicationLaunchedRequest request, NetworkCredential credentials);
        void AutoDiscoveryFinished(IAutoDiscoveryFinishedRequest request, NetworkCredential credentials);
        void ScanStarted(IScanStartedRequest request,NetworkCredential credentials);
        void ScanFinished(IScanFinishedRequest request, NetworkCredential credentials);
        void ScanUpdated(IScanUpdatedRequest request, NetworkCredential credentials);
        void ReportGenerated(IReportGeneratedRequest request, NetworkCredential credentials);
        void ReportPrintedOrSaved(IReportPrintedOrSavedRequest request, NetworkCredential credentials);
        void ApplicationClosed(IApplicationClosedRequest request, NetworkCredential credentials);
    }

    
}
