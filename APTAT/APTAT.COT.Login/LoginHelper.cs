using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;

namespace APTAT.COT.Login
{
    public sealed class LoginHelper
    {
        private const string MAIN_URL = "https://one.comodo.com/app/sso";
        private const string LOGIN_ENDPOINT = "/api/auth";
        private const string CHECK_ENDPOINT = "/check-auth";


        public static LoginResults Login(SecurityCamCredentials userCredentials)
        {
            var jsonString = string.Empty;

            using (var resString = LoginAsync(userCredentials))
            {
                resString.Wait();
                jsonString = resString.Result;
            }

            var jsonData = ParseJson<LoginRootObject>(jsonString);

            var result = LoginResults.ERR_AUTHENTICATION;
            if (jsonData != null)
            {
                if (!Enum.TryParse(jsonData.code, out result))
                {
                    result = LoginResults.ERR_AUTHENTICATION;
                }
            }

            return result;
        }

        public static CheckAuthResults CheckAuth(SecurityCamCredentials userCredentials)
        {
            var jsonString = string.Empty;
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
                return null;
            }

            return root;
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
                using (var response = await Request(checkHttpClient, HttpMethod.Get, url, string.Empty, pHeaders))
                {
                    using (var httpContent = response.Content)
                    {
                        responseText = await httpContent.ReadAsStringAsync();
                    }
                }

            }
            return responseText;
        }

        private static async Task<string> LoginAsync(SecurityCamCredentials userCredentials)
        {
            string responseText = string.Empty;
            using (var loginHttpClient = new HttpClient())
            {
                loginHttpClient.BaseAddress = new Uri(MAIN_URL);
                loginHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var pHeaders = GetHeaders(userCredentials);

                var content = string.Format("{{\"email\": \"{0}\",\"password\": \"{1}\",\"nameSpace\": \"{2}\"}}", userCredentials.Login
                                                                                                                 , userCredentials.Password.ConvertToString()
                                                                                                                 , userCredentials.Namespace);

                string url = MAIN_URL + LOGIN_ENDPOINT;


                using (var response = await Request(loginHttpClient, HttpMethod.Post, url, content, pHeaders))
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
        static Task<HttpResponseMessage> Request(HttpClient client, HttpMethod pMethod, string pUrl, string pJsonContent, Dictionary<string, string> pHeaders)
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
            HttpContent httpContent = null;
            Task<HttpResponseMessage> message = null;
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
            }

            message.Wait();
            if (message.Result.StatusCode == HttpStatusCode.OK)
            {
                var task = message.Result.Content.ReadAsStringAsync();
                task.Wait();
                string strRes = task.Result;
            }
            return message;
        }


        public static bool Logout()
        {
            //TODO: Implement Logout
            return true;
        }

    }
}

