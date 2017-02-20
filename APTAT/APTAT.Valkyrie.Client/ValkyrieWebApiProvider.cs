using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Valkyrie.WebApiProvider.Contracts;
using Valkyrie.WebApiProvider.Enums;
using Valkyrie.WebApiProvider.Extensions;
using Valkyrie.WebApiProvider.Helpers;

namespace Valkyrie.WebApiProvider
{

    public class ValkyrieWebApiProvider : IValkyrieWebApiProvider
    {
        #region Urls
        private Uri BasicInfoUri => new Uri(_valkyrieBaseUri, "fvs_basic_info");
        private Uri FullInfoUri => new Uri(_valkyrieBaseUri, "fvs_full_info");
        private Uri SubmitToManualAnalysisUri => new Uri(_valkyrieBaseUri, "fvs_submit_manual");
        private Uri SubmitToManualAnalysisWithoutFileUploadUri => new Uri(_valkyrieBaseUri, "fvs_submit_manual_sha1");
        private Uri SubmitToAutomaticAnalysisUri => new Uri(_valkyrieBaseUri, "fvs_submit_auto");
        private Uri ManualStatusUri => new Uri(_valkyrieBaseUri, "fvs_manual_status");
        private Uri RegisterWithMachineIdUri => new Uri(_valkyrieBaseUri, "fvs_register_with_machine_id");
        private Uri FinalizeScanUri => new Uri(_valkyrieBaseUri, "apt_finalize_scan");
        private Uri StartScanUri => new Uri(_valkyrieBaseUri, "apt_start_scan");
        private Uri EndScanUri => new Uri(_valkyrieBaseUri, "apt_end_scan");
        private Uri RegisterWithCamLicenseUri => new Uri(_valkyrieBaseUri, "fvs_register_with_cam_license");
        private Uri RegisterWithCamCredentialsUri => new Uri(_valkyrieBaseUri, "fvs_register_with_cam_credentials");
        private Uri CheckAnalysisResultUri => new Uri(_valkyrieBaseUri, "fvs_check_analysis_result");
        private Uri FatSubmitFilePaths => new Uri(_valkyrieBaseUri, "fat_submit_file_paths");
        private Uri FatEmailSubstcription => new Uri(_valkyrieBaseUri, "fat_email_subscription");
        private bool IsProduction => _valkyrieBaseUri.ToString().ToUpper().Contains("staging") == false;

        #endregion
        
        private Uri _valkyrieBaseUri;

        
        /// <summary>
        /// This request will be used to get Basic Info of a file that FVS have already analyzed before. 
        /// </summary>             
        public BasicInfoResponse GetBasicInfo(InfoRequest request)
        {
            var response = CallValkyrie<BasicInfoResponse>(BasicInfoUri, request);
            return response;
        }

        // ReSharper disable once InconsistentNaming
        public RegisterWithResponse RegisterWithMachineId(RegisterWithMachineIdRequest request)
        {
            var response = CallValkyrie<RegisterWithResponse>(RegisterWithMachineIdUri, request);
            return response;
        }

        public RegisterWithResponse RegisterWithCamLicense(RegisterWithCamLicenseRequest request)
        {
            var response = CallValkyrie<RegisterWithResponse>(RegisterWithCamLicenseUri, request);
            return response;
        }

        public RegisterWithResponse RegisterWithCamCredentials(RegisterWithCamCredentials request)
        {
            var response = CallValkyrie<RegisterWithResponse>(RegisterWithCamCredentialsUri, request);
            return response;
        }
        
        public StartAndFinalizeScanResponse StartScan(StartScanRequest request)
        {
            var response = CallValkyrie<StartAndFinalizeScanResponse>(StartScanUri, request);
            return response;
        }

        public StartAndFinalizeScanResponse EndScan(EndScanRequest request)
        {
            StartAndFinalizeScanResponse response = null;
            try
            {
                response = CallValkyrie<StartAndFinalizeScanResponse>(EndScanUri, request);
            }
            catch (Exception)
            {
            }
            return response;
        }
        public StartAndFinalizeScanResponse EndScanWithErrorHandling(EndScanRequest request)
        {
            bool success = false;
            int retry = 0;
            StartAndFinalizeScanResponse response = new StartAndFinalizeScanResponse();
            const int interval = 10000;
            const int timeout = 180000;
            int attempts = timeout / interval;

            while (!success && retry < attempts)
            {
                try
                {
                    response = CallValkyrie<StartAndFinalizeScanResponse>(EndScanUri, request);
                    success = response.ReturnCode == ErrorCodesEnum.Success
                        || response.ReturnCode == ErrorCodesEnum.AptNoStartedScanFound;
                }
                catch (Exception)
                {
                }
                finally
                {
                    retry++;
                }
                Thread.Sleep(interval);
            }
            return response;
        }


        public FullInfoResponse GetFullInfo(InfoRequest request)
        {
            var result = CallValkyrie<FullInfoResponse>(FullInfoUri, request);
            return result;
        }

        /// <summary>
        /// This request will be used to . This request is  request and following form data must Submit a File to Manual Analysis HTTP POST be provided with corresponding keys: 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filePath"></param>
        /// <param name="token"></param>
        /// <returns></returns>

        public SubmitToManualResponse SubmitToManualAnalysis(SubmitToManualRequest request, string filePath, CancellationToken token)
        {
            if (token == null)
                token = CancellationToken.None;

            string originalFileName = Path.GetFileName(request.FilePath);
            var payload = request.ToNameValueCollection();
            var uploadResult = MultipartPostHelper.Post(filePath, SubmitToManualAnalysisUri.AbsoluteUri, payload, originalFileName, token);
            var response = JsonConvert.DeserializeObject<SubmitToManualResponse>(uploadResult);
            if (response.ReturnCode != ErrorCodesEnum.Success)
            {
                throw new InvalidOperationException(response.ReturnCode.GetEnumDescription());
            }

            return response;
        }

        public SubmitToManualResponse SubmitToManualAnalysisWithoutUpload(SubmitToManualRequestSHA1 request)
        {
            var result = CallValkyrie<SubmitToManualResponse>(SubmitToManualAnalysisWithoutFileUploadUri, request);
            return result;
        }

        //public static SubmitToManualResponse SubmitToManualAnalysis(SubmitToManualRequest request, byte[] fileData, CancellationToken token)
        //{
        //    if (token == null)
        //        token = CancellationToken.None;

        //    string originalFileName = Path.GetFileName(request.FilePath);
        //    var payload = request.ToNameValueCollection();
        //    var uploadResult = MultipartPostHelper.Post(fileData, ValkyrieApiUrls.SubmitToManualAnalysis.AbsoluteUri, payload, originalFileName, token);
        //    var response = JsonConvert.DeserializeObject<SubmitToManualResponse>(uploadResult);
        //    if (response.ReturnCode != ErrorCodesEnum.Success)
        //    {
        //        throw new InvalidOperationException(response.ReturnCode.GetEnumDescription());
        //    }

        //    return response;
        //}

        public SubmitToAutomaticResponse SubmitToAutomaticAnalysis(SubmitToAutomaticRequest request, FileStream fileStream, CancellationToken token, String originalFilePath)
        {
            var payload = request.ToNameValueCollection();
            var uploadResult = MultipartPostHelper.Post(fileStream, SubmitToAutomaticAnalysisUri.AbsoluteUri, payload, originalFilePath, token);
            var response = JsonConvert.DeserializeObject<SubmitToAutomaticResponse>(uploadResult);
            if (response.ReturnCode != ErrorCodesEnum.Success)
            {
                throw new InvalidOperationException(response.ReturnCode.GetEnumDescription());
            }

            return response;
        }

        //public static SubmitToAutomaticResponse SubmitToAutomaticAnalysis(SubmitToAutomaticRequest request, string filePath, CancellationToken token)
        //{
        //    if (token == null)
        //    {
        //        token = CancellationToken.None;
        //    }

        //    string originalFileName = Path.GetFileName(request.FilePath);
        //    var payload = request.ToNameValueCollection();
        //    var uploadResult = MultipartPostHelper.Post(filePath, ValkyrieApiUrls.SubmitToAutomaticAnalysis.AbsoluteUri, payload, originalFileName, token);
        //    var response = JsonConvert.DeserializeObject<SubmitToAutomaticResponse>(uploadResult);
        //    if (response.ReturnCode != ErrorCodesEnum.Success)
        //    {
        //        throw new InvalidOperationException(response.ReturnCode.GetEnumDescription());
        //    }

        //    return response;
        //}

        #region Fat Methods

        public ValkyrieResponse FatSubmitFiles(FatSubmitFilePathsRequest request)
        {
            var result = CallValkyrie<ValkyrieResponse>(FatSubmitFilePaths, request);
            return result;
        }

        #endregion

        public ManualStatusResponse ManualStatus(ManualStatusRequest request)
        {
            var result = CallValkyrie<ManualStatusResponse>(ManualStatusUri, request);
            return result;
        }

        public CheckAnalysisResultResponse CheckAnalysisResult(CheckAnalysisResultRequest request)
        {
            var result = CallValkyrie<CheckAnalysisResultResponse>(CheckAnalysisResultUri, request);
            return result;
        }

        private T CallValkyrie<T>(Uri apiUri, object request)
        {
            Trace.WriteLine($"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : Call {apiUri}");
            var payload = request.ToNameValueCollection();
            using (WebClient client = new WebClient())
            {
                

                
                if (ProxySettings.Proxy != null)
                {
                    client.Proxy = ProxySettings.Proxy;
                }
                byte[] response = client.UploadValues(apiUri, payload);
                string responseEncoded = System.Text.Encoding.UTF8.GetString(response);
                Trace.WriteLine($"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : Response for {apiUri} {Environment.NewLine}{responseEncoded}");
                var result = JsonConvert.DeserializeObject<T>(responseEncoded);
                return result;
            }
        }

        public ValkyrieResponse SubmitEmail(SubscribeEmailRequest request)
        {
            ValkyrieResponse result;
            try
            {
                result = CallValkyrie<ValkyrieResponse>(FatEmailSubstcription, request);
            }
            catch (Exception e)
            {
                result = new ValkyrieResponse() {ResultMessage = e.Message, Error = e, ReturnCode = ErrorCodesEnum.None};
            }
            
            return result;
        }

        public Uri GetBaseUri()
        {
            return _valkyrieBaseUri;
        }

        public void SetBaseUri(Uri uri)
        {
            _valkyrieBaseUri = uri;
        }
    }
}
