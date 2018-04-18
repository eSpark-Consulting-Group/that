using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.COT.Login
{
    public class LoginHelper2
    {
        private static readonly HttpClient client = new HttpClient();

        public static void LoginTest()
        {
            new LoginHelper2().Login(new SecurityUserCredentials("email", "password")).Wait();
        }

        public async Task<string> Login(SecurityUserCredentials userCredentials)
        {
            client.BaseAddress = new Uri("https://staging.one.comodo.com/app/sso/");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            var values = new Dictionary<string, string>
                {
                    { "email", "olga.tumanova@comodo.com" },
                    { "password", "Comodo" }
                };

            var content = "{\"email\": \"olga.tumanova@comodo.com\",\"password\": \"Comodo\",\"nameSpace\": \"C1T\"}";
            
            string url = "https://staging.one.comodo.com/app/sso/api/auth";
            var response = await Request(HttpMethod.Post, url, content, new Dictionary<string, string>());
            string responseText = await response.Content.ReadAsStringAsync();

            return responseText;
            //var responseString = await response.Content.ReadAsStringAsync();
            //    //TODO: Implement Login
            //    //            string POSTRequest =
            //    //string.Format($"{
            //    //    "email\": "{userCredentials.Login}",
            //    //    "password\": "{ userCredentials.Password.ConvertToString()}",
            //    //    "nameSpace\": "{ userCredentials.Namespace}"
            //    //}";
            //return LoginResults.SUCCESS_AUTHENTICATION;
        }

        /// <summary>
        /// Makes an async HTTP Request
        /// </summary>
        /// <param name="pMethod">Those methods you know: GET, POST, HEAD, etc...</param>
        /// <param name="pUrl">Very predictable...</param>
        /// <param name="pJsonContent">String data to POST on the server</param>
        /// <param name="pHeaders">If you use some kind of Authorization you should use this</param>
        /// <returns></returns>
        static async Task<HttpResponseMessage> Request(HttpMethod pMethod, string pUrl, string pJsonContent, Dictionary<string, string> pHeaders)
        {
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Method = pMethod;
            httpRequestMessage.RequestUri = new Uri(pUrl);
            foreach (var head in pHeaders)
            {
                httpRequestMessage.Headers.Add(head.Key, head.Value);
            }
            switch (pMethod.Method)
            {
                case "POST":
                    HttpContent httpContent = new StringContent(pJsonContent, Encoding.UTF8, "application/json");
                    httpRequestMessage.Content = httpContent;
                    break;

            }

            return await client.SendAsync(httpRequestMessage);
        }
    

    public bool Logout()
        {
            //TODO: Implement Logout
            return true;
        }
        
    }
}
