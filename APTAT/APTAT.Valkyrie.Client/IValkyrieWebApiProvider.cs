using System;
using System.IO;
using System.Threading;
using Valkyrie.WebApiProvider.Contracts;

namespace Valkyrie.WebApiProvider
{
    public interface IValkyrieWebApiProvider
    {
        CheckAnalysisResultResponse CheckAnalysisResult(CheckAnalysisResultRequest request);
        StartAndFinalizeScanResponse EndScan(EndScanRequest request);
        Uri GetBaseUri();
        BasicInfoResponse GetBasicInfo(InfoRequest request);
        FullInfoResponse GetFullInfo(InfoRequest request);
        ManualStatusResponse ManualStatus(ManualStatusRequest request);
        RegisterWithResponse RegisterWithCamCredentials(RegisterWithCamCredentials request);
        RegisterWithResponse RegisterWithCamLicense(RegisterWithCamLicenseRequest request);
        RegisterWithResponse RegisterWithMachineId(RegisterWithMachineIdRequest request);
        void SetBaseUri(Uri uri);
        StartAndFinalizeScanResponse StartScan(StartScanRequest request);
        SubmitToAutomaticResponse SubmitToAutomaticAnalysis(SubmitToAutomaticRequest request, FileStream fileStream, CancellationToken token, string originalFilePath);
        SubmitToManualResponse SubmitToManualAnalysis(SubmitToManualRequest request, string filePath, CancellationToken token);
        SubmitToManualResponse SubmitToManualAnalysisWithoutUpload(SubmitToManualRequestSHA1 request);
        ValkyrieResponse FatSubmitFiles(FatSubmitFilePathsRequest request);
        ValkyrieResponse SubmitEmail(SubscribeEmailRequest request);
    }
}