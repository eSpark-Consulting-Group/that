using APTAT.Valkyrie.Client.Contracts;
using APTAT.Valkyrie.Client.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        public static RegisterWithMachineIdResponse RegisterWithMachineId(RegisterWithMachineIdRequest request)
        {
            var response = CallValkyrie<RegisterWithMachineIdResponse>(ValkyrieApiUrls.RegisterWithMachineId, request);
            return response;
        }

        public static StartAndFinalizeScanResponse StartScan(StartScanRequest request)
        {
            var response = CallValkyrie<StartAndFinalizeScanResponse>(ValkyrieApiUrls.StartScan, request);
            return response;
        }

        public static StartAndFinalizeScanResponse EndScan(EndScanRequest request)
        {
            var response = CallValkyrie<StartAndFinalizeScanResponse>(ValkyrieApiUrls.EndScan, request);
            return response;
        }

        

        public static FullInfoResponse GetFullInfo(InfoRequest request)
        {
            var result = CallValkyrie<FullInfoResponse>(ValkyrieApiUrls.FullInfo, request);
            return result;
        }

        public static SubmitToManualResponse SubmitToManualAnalysis(SubmitToManualRequest request)
        {
            var result = CallValkyrie<SubmitToManualResponse>(ValkyrieApiUrls.SubmitToManualAnalysis, request);
            return result;
        }


        //public static SubmitToAutomaticResponse SubmitToAutomaticAnalysis(SubmitToAutomaticRequest request)
        //{
        //    var payload = request.ToNameValueCollection();
        //    var uploadResult = MultipartPostHelper.Post(request.FilePath, ValkyrieApiUrls.SubmitToAutomaticAnalysis.AbsoluteUri, payload);
        //    var result = JsonConvert.DeserializeObject<SubmitToAutomaticResponse>(uploadResult);
        //    return result;
        //}

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
#if DEBUG
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
#endif
                client.Proxy = WebRequest.GetSystemWebProxy();
                byte[] response = client.UploadValues(apiUri, payload);
                string responseEncoded = System.Text.Encoding.UTF8.GetString(response);
                var result = JsonConvert.DeserializeObject<T>(responseEncoded);
                return result;
            }
        }
    }
}
