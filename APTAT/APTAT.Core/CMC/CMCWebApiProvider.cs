using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace APTAT.Core.CMC
{
    public class CmcWebApiProvider:ICmcWebApiProvider
    {
        private Uri _cmcBaseUri;

        public Uri GetBaseUri()
        {
            return _cmcBaseUri;
        }

        public void SetBaseUri(Uri uri)
        {
            _cmcBaseUri = uri;
        }

        static string Get(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                }
                throw;
            }
        }

        static void Post(string url, Object obj)
        {
            string jsonContent =  JsonConvert.SerializeObject(obj);
            

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(jsonContent);
             
            request.ContentLength = byteArray.Length;
            request.ContentType = @"application/json";

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
            long length = 0;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    length = response.ContentLength;
                }
            }
            catch (WebException ex){  }
        }

        public void SendData(IGeneralInfo request)
        {
            Post(_cmcBaseUri.ToString(), request);
        }
    }
}
