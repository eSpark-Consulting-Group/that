using APTAT.Valkyrie.Client.Contracts;
using APTAT.Valkyrie.Client.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace APTAT.Valkyrie.Client
{

    public static class ValkyrieService
    {

        /// <summary>
        /// This request will be used to get Basic Info of a file that FVS have already analyzed before. 
        /// </summary>             
        public static BasicInfoResponse GetBasicInfo(InfoRequest request)
        {
            var response = CallValkyrie<BasicInfoResponse>(ValkyrieApiUrls.BasicInfo, request);
            return response;
        }

        // ReSharper disable once InconsistentNaming
        internal static RegisterWithResponse RegisterWithMachineId(RegisterWithMachineIdRequest request)
        {
            var response = CallValkyrie<RegisterWithResponse>(ValkyrieApiUrls.RegisterWithMachineId, request);
            return response;
        }

        internal static RegisterWithResponse RegisterWithCamLicense(RegisterWithCamLicenseRequest request)
        {
            var response = CallValkyrie<RegisterWithResponse>(ValkyrieApiUrls.RegisterWithCamLicense, request);
            return response;
        }

        internal static RegisterWithResponse RegisterWithCamCredentials(RegisterWithCamCredentials request)
        {
            var response = CallValkyrie<RegisterWithResponse>(ValkyrieApiUrls.RegisterWithCamCredentials, request);
            return response;
        }

        internal static StartAndFinalizeScanResponse StartScan(StartScanRequest request)
        {
            var response = CallValkyrie<StartAndFinalizeScanResponse>(ValkyrieApiUrls.StartScan, request);
            return response;
        }

        internal static StartAndFinalizeScanResponse EndScan(EndScanRequest request)
        {
            var response = CallValkyrie<StartAndFinalizeScanResponse>(ValkyrieApiUrls.EndScan, request);
            return response;
        }


        internal static FullInfoResponse GetFullInfo(InfoRequest request)
        {
            var result = CallValkyrie<FullInfoResponse>(ValkyrieApiUrls.FullInfo, request);
            return result;
        }

        /// <summary>
        /// This request will be used to . This request is  request and following form data must Submit a File to Manual Analysis HTTP POST be provided with corresponding keys: 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filePath"></param>
        /// <param name="token"></param>
        /// <returns></returns>

        public static SubmitToManualResponse SubmitToManualAnalysis(SubmitToManualRequest request, string filePath, CancellationToken token)
        {
            if (token == null)
                token = CancellationToken.None;

            string originalFileName = Path.GetFileName(request.FilePath);
            var payload = request.ToNameValueCollection();
            var uploadResult = APTAT.Valkyrie.Client.Helpers.MultipartPostHelper.Post(filePath, ValkyrieApiUrls.SubmitToManualAnalysis.AbsoluteUri, payload, originalFileName, token);
            var response = JsonConvert.DeserializeObject<SubmitToManualResponse>(uploadResult);
            if (response.ReturnCode != ErrorCodesEnum.Success)
            {
                throw new InvalidOperationException(response.ReturnCode.GetEnumDescription());
            }

            return response;
        }

        public static SubmitToManualResponse SubmitToManualAnalysisWithoutUpload(SubmitToManualRequestSHA1 request)
        {
            var result = CallValkyrie<SubmitToManualResponse>(ValkyrieApiUrls.SubmitToManualAnalysisWithoutFileUpload, request);
            return result;            
        }
        
        public static SubmitToManualResponse SubmitToManualAnalysis(SubmitToManualRequest request, byte[] fileData, CancellationToken token)
        {
            if (token == null)
                token = CancellationToken.None;

            string originalFileName = Path.GetFileName(request.FilePath);
            var payload = request.ToNameValueCollection();
            var uploadResult = APTAT.Valkyrie.Client.Helpers.MultipartPostHelper.Post(fileData, ValkyrieApiUrls.SubmitToManualAnalysis.AbsoluteUri, payload, originalFileName, token);
            var response = JsonConvert.DeserializeObject<SubmitToManualResponse>(uploadResult);
            if (response.ReturnCode != ErrorCodesEnum.Success)
            {
                throw new InvalidOperationException(response.ReturnCode.GetEnumDescription());
            }

            return response;
        }

        public static SubmitToAutomaticResponse SubmitToAutomaticAnalysis(SubmitToAutomaticRequest request, string filePath, CancellationToken token)
        {
            if (token == null)
            {
                token = CancellationToken.None;
            }

            string originalFileName = Path.GetFileName(request.FilePath);
            var payload = request.ToNameValueCollection();
            var uploadResult = APTAT.Valkyrie.Client.Helpers.MultipartPostHelper.Post(filePath, ValkyrieApiUrls.SubmitToAutomaticAnalysis.AbsoluteUri, payload, originalFileName, token);
            var response = JsonConvert.DeserializeObject<SubmitToAutomaticResponse>(uploadResult);
            if (response.ReturnCode != ErrorCodesEnum.Success)
            {
                throw new InvalidOperationException(response.ReturnCode.GetEnumDescription());
            }

            return response;
        }

        public static ManualStatusResponse ManualStatus(ManualStatusRequest request)
        {
            var result = CallValkyrie<ManualStatusResponse>(ValkyrieApiUrls.ManualStatus, request);
            return result;
        }


        private static T CallValkyrie<T>(Uri apiUri, object request)
        {
            var payload = request.ToNameValueCollection();
            using (WebClient client = new WebClient())
            {

                if (!ValkyrieApiUrls.IsProduction)
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                }
                if (ProxySettings.Proxy != null)
                {
                    client.Proxy = ProxySettings.Proxy;
                }
                byte[] response = client.UploadValues(apiUri, payload);
                string responseEncoded = System.Text.Encoding.UTF8.GetString(response);
                var result = JsonConvert.DeserializeObject<T>(responseEncoded);
                return result;
            }
        }
    }
}
