using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Diagnostics;
using APTAT.Common;
using System.Collections.Specialized;
using APTAT.Core;
using Microsoft.VisualBasic;

namespace APTAT.COT.Login
{
    public sealed class LoginHelper
    {
        private const string MAIN_URL = "https://one.comodo.com/app/sso";
        private const string MAIN_URL_US = "https://one-us.comodo.com/app/sso";
        private const string LOGIN_ENDPOINT = "/api/auth";
        private const string CHECK_ENDPOINT = "/check-auth";
        private const string OTP_ENDPOINT = "/api/verify-otp";

        private static string REGION = "";
        public static string otpKey = "";

        private static LoginResults ParseLoginResult(string jsonString)
        {
            var jsonData = ParseJson<LoginRootObject>(jsonString);

            var result = LoginResults.ERR_AUTHENTICATION;
            if (jsonData != null)
            {
                if (!Enum.TryParse(jsonData.code, out result))
                {
                    result = LoginResults.ERR_AUTHENTICATION;
                    LogHelper.Trace(TraceEventType.Information, "Result: {0}, Token: {1}", jsonData?.code, jsonData?.data?.token);
                }
            }
            
            return result;
        }

        public static async Task<LoginResults> Login(SecurityCamCredentials userCredentials)
        {
            string jsonString;

            REGION = MAIN_URL;
            using (var resString = LoginAsync(userCredentials, REGION))
            {
                await resString;
                jsonString = resString.Result;
            }

            var result = ParseLoginResult(jsonString);
            
            if (result == LoginResults.ERR_AUTHENTICATION)
                return result;

            if (result == LoginResults.ERR_USER_NOT_FOUND)
            {
                /* Let's try US server */
                jsonString = "";
                REGION = MAIN_URL_US;
                using (var resString = LoginAsync(userCredentials, REGION))
                {
                    await resString;
                    jsonString = resString.Result;
                }

                result = ParseLoginResult(jsonString);
                if (result != LoginResults.OTP_REQUIRED)
                    return result;
            }
            if (result == LoginResults.OTP_REQUIRED)
            {
                /* get otpKey from json request */
                var jsonData = ParseJson<LoginOtpObject>(jsonString);
                otpKey = jsonData.data.otpKey;
                REGION += OTP_ENDPOINT;
                return result;
            }
                return result;
        }

        public static async Task<LoginResults> CheckTwoFactorAuth(string TwoFacInput) // input 
        {
            /* verify with the server */
            string jsonResp;
            using (var resString = Verify2fa(TwoFacInput, otpKey, REGION))
            {
                await resString;
                jsonResp = resString.Result;
            }

            return ParseLoginResult(jsonResp);
        }

        public static CheckAuthResults CheckAuth(SecurityCamCredentials userCredentials)
        {
            var jsonString = string.Empty;
            /* TODO: CheckAuth only supports EU server, since it is not used anywhere
             * we don't modify it. If we use CheckAuth someday, we need to add support
             * for US server
             */
            using (var resString = CheckAuthAsync(userCredentials))
            {
                resString.Wait();
                jsonString = resString.Result;
            }

            var jsonData = ParseJson<CheckRootObject>(jsonString);

            var result = CheckAuthResults.ERR_MISSING_PARAM;
            if (jsonData != null)
            {
                if (!Enum.TryParse(jsonData.code, out result))
                {
                    result = CheckAuthResults.ERR_MISSING_PARAM;
                }
            }

            return result;
        }

        private static TResult ParseJson<TResult>(string jsonRes) where TResult : class, new()
        {
            var jsonSerSettings = new DataContractJsonSerializerSettings();
            var jsonSerializer = new DataContractJsonSerializer(typeof(TResult), jsonSerSettings);

            var root = default(TResult);
            try
            {
                using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonRes)))
                {
                    root = jsonSerializer.ReadObject(memStream) as TResult;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }

            return root;
        }

        private static async Task<string> Verify2fa(string code, string otpKey, string endpoint)
        {
            string responseText = string.Empty;
            using (var checkHttpClient = new HttpClient())
            {
                checkHttpClient.BaseAddress = new Uri(endpoint);
                checkHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var jsonContent = string.Format("{{\"otpKey\": \"{0}\",\"otp\": \"{1}\"}}", otpKey, code);
                using (var response = Request(checkHttpClient, HttpMethod.Post, endpoint, jsonContent, new Dictionary<string, string>()))
                {
                    using (var httpContent = response.Content)
                    {
                        responseText = await httpContent.ReadAsStringAsync();
                    }
                }

            }
            return responseText;
        }

        private static async Task<string> CheckAuthAsync(SecurityCamCredentials userCredentials)
        {
            string responseText = string.Empty;
            using (var checkHttpClient = new HttpClient())
            {
                checkHttpClient.BaseAddress = new Uri(MAIN_URL);
                checkHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                var pHeaders = GetHeaders(userCredentials);

                string url = MAIN_URL + CHECK_ENDPOINT;
                using (var response = Request(checkHttpClient, HttpMethod.Get, url, string.Empty, pHeaders))
                {
                    using (var httpContent = response.Content)
                    {
                        responseText = await httpContent.ReadAsStringAsync();
                    }
                }

            }
            return responseText;
        }

        public static Tuple<bool, string> CheckCamLicenseID(string licenseId, string productName, string productVersion)
        {
            return CheckCamLicenseIDInternal(licenseId, productName, productVersion);
        }

        private static Tuple<bool, string> CheckCamLicenseIDInternal(string licenseId, string productName, string productVersion)
        {
            var baseUrl = Core.Properties.Settings.Default.valkyrieBaseUrl;

            var endPoint = "fvs_register_with_cam_license";
            var url = baseUrl.TrimEnd('/') + "/" + endPoint;
                       
            string responseText = string.Empty;           

            var namedCollection = new NameValueCollection
            {
                { "license_id", licenseId },
                { "product_name", productName },
                { "product_version", productVersion },
                { "api_key", string.Empty }
            };

            using (WebClient client = new WebClient())
            {
                byte[] response = client.UploadValues(url, namedCollection);

                responseText = System.Text.Encoding.UTF8.GetString(response);
                Trace.WriteLine($"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : Response for {url} {Environment.NewLine}{responseText}");
            }


            var root = ParseJson<ValkiryeLicenseIDCheckResult>(responseText);

            return new Tuple<bool, string>(root.result_message == "SUCCESS", root.result_description);
        }

        private static async Task<string> LoginAsync(SecurityCamCredentials userCredentials, String loginUrl)
        {
            string responseText = string.Empty;
            using (var loginHttpClient = new HttpClient())
            {
                loginHttpClient.BaseAddress = new Uri(loginUrl);
                loginHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var pHeaders = GetHeaders(userCredentials);

                var content = string.Format("{{\"email\": \"{0}\",\"password\": \"{1}\",\"nameSpace\": \"{2}\"}}", userCredentials.Login
                                                                                                                 , userCredentials.Password.ConvertToString()
                                                                                                                 , userCredentials.Namespace);

                string url = loginUrl + LOGIN_ENDPOINT;         
                using (var response = Request(loginHttpClient, HttpMethod.Post, url, content, pHeaders))
                {
                    using (var httpContent = response.Content)
                    {
                        responseText = await httpContent.ReadAsStringAsync();
                    }
                }
            }

            return responseText;
        }

        private static Dictionary<string, string> GetHeaders(SecurityCamCredentials userCredentials)
        {
            return new Dictionary<string, string>
                {
                    { "email", userCredentials.Login },
                    { "password", userCredentials.Password.ConvertToString() },
                    { "nameSpace", userCredentials.Namespace }
                };
        }

        /// <summary>
        /// Makes an async HTTP Request
        /// </summary>
        /// <param name="pMethod">Those methods you know: GET, POST, HEAD, etc...</param>
        /// <param name="pUrl">Very predictable...</param>
        /// <param name="pJsonContent">String data to POST on the server</param>
        /// <param name="pHeaders">If you use some kind of Authorization you should use this</param>
        /// <returns></returns>
        static HttpResponseMessage Request(HttpClient client, HttpMethod pMethod, string pUrl, string pJsonContent, Dictionary<string, string> pHeaders)
        {
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = pMethod,
                RequestUri = new Uri(pUrl)
            };
            foreach (var head in pHeaders)
            {
                httpRequestMessage.Headers.Add(head.Key, head.Value);
            }
            HttpContent httpContent;
            Task<HttpResponseMessage> message;
            switch (pMethod.Method)
            {
                case "POST":
                    httpContent = new StringContent(pJsonContent, Encoding.UTF8, "application/json");
                    httpRequestMessage.Content = httpContent;
                    message = client.SendAsync(httpRequestMessage);
                    break;
                case "GET":
                    httpContent = new StringContent(pJsonContent, Encoding.UTF8, "text/html");
                    httpRequestMessage.Content = httpContent;
                    message = client.GetAsync(pUrl);
                    break;
                    default:
                        return null;
            }

            message.Wait(20000);
            return message.Result;
        }

        public static bool Logout()
        {
            //TODO: Implement Logout
            return true;
        }
    }
}

