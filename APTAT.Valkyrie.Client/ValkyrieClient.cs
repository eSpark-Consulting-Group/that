using APTAT.Valkyrie.Client.Contracts;
using Newtonsoft.Json;
using System;
using System.Net;
namespace APTAT.Valkyrie.Client
{

    public static class ValkyrieClient
    {
        /// <summary>
        /// This request will be used to get Basic Info of a file that FVS have already analyzed before. 
        /// </summary>             
        public static BasicInfoResponse GetBasicInfo(BasicInfoRequest request)
        {                     
            var response = CallValkyrie<BasicInfoResponse>(ValkyrieApiUrls.BasicInfo, request);
            return response;
        }

        public static FullInfoResponse GetFullInfo(FullInfoRequest request)
        {            
            var result = CallValkyrie<FullInfoResponse>(ValkyrieApiUrls.FullInfo, request);
            return result;
        }

        public static SubmitToManualResponse SubmitToManualAnalysis(SubmitToManualRequest request)
        {
            var result = CallValkyrie<SubmitToManualResponse>(ValkyrieApiUrls.SubmitToManual, request);
            return result;
        } 

        private static T CallValkyrie<T>(Uri apiUri, RequestBase request)
        {
            var payload = request.ToNameValueCollection();
            using (WebClient client = new WebClient())
            {                                
                byte[] response = client.UploadValues(apiUri, payload);                
                string responseEncoded = System.Text.Encoding.UTF8.GetString(response);
                var result = JsonConvert.DeserializeObject<T>(responseEncoded);
                return result;
            }
        }
    }
}
