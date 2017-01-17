using System.IO;
using System.Threading;
using Valkyrie.WebApiProvider.Contracts;

namespace Valkyrie.WebApiProvider
{
    public interface IValkyrieWebApiProvider
    {
        CheckAnalysisResultResponse CheckAnalysisResult(CheckAnalysisResultRequest request);
        StartAndFinalizeScanResponse EndScan(EndScanRequest request);
        BasicInfoResponse GetBasicInfo(InfoRequest request);
        RegisterWithResponse RegisterWithCamCredentials(RegisterWithCamCredentials request);
        RegisterWithResponse RegisterWithCamLicense(RegisterWithCamLicenseRequest request);
        StartAndFinalizeScanResponse StartScan(StartScanRequest request);
        SubmitToAutomaticResponse SubmitToAutomaticAnalysis(SubmitToAutomaticRequest request, FileStream fileStream, CancellationToken token);
    }
}