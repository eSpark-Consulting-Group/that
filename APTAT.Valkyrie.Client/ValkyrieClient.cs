using APTAT.Valkyrie.Client.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
namespace APTAT.Valkyrie.Client
{

    public static class ValkyrieClient
    {
        /// <summary>
        /// This request will be used to get Basic Info of a file that FVS have already analyzed before. 
        /// </summary>             
        public static BasicInfoResponse GetBasicInfo(InfoRequest request)
        {                     
            var response = CallValkyrie<BasicInfoResponse>(ValkyrieApiUrls.BasicInfo, request);
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


        public static SubmitToAutomaticResponse SubmitToAutomaticAnalysis(SubmitToAutomaticRequest request)
        {
            var result = CallValkyrie<SubmitToAutomaticResponse>(ValkyrieApiUrls.SubmitToAutomaticAnalysis, request);
            return result;
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
#if DEBUG
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
#endif
                byte[] response = client.UploadValues(apiUri, payload);                
                string responseEncoded = System.Text.Encoding.UTF8.GetString(response);
                var result = JsonConvert.DeserializeObject<T>(responseEncoded);
                return result;
            }
        }
    }
}
