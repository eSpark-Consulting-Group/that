using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Configuration;
using System.Text;
using APTAT.Common;
using Newtonsoft.Json;
using Valkyrie.WebApiProvider;
using APTAT.Core.Properties;

namespace APTAT.Core.CMC
{
    public class CmcWebApiProvider: ICmcWebApiProvider
    {
        private Uri _cmcBaseUri;
        public Uri GetBaseUri() { return _cmcBaseUri;}
        public void SetBaseUri(Uri uri) { _cmcBaseUri = uri; }

        public void SendData(IGeneralInfo request,NetworkCredential networkCredential) {
            SetProxy(networkCredential);
            Post(_cmcBaseUri.ToString(), request);
        }

        public CmcWebApiProvider() {
            _cmcBaseUri = new Uri(Settings.Default.cmcWebApiProviderBaseUrl);
        }

        private void SetProxy(NetworkCredential networkCredential)
        {
            var proxy = WebRequest.GetSystemWebProxy();
            if (networkCredential != null)
                proxy.Credentials = networkCredential;
            ProxySettings.Proxy = proxy;
        }

        static string Get(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Proxy = ProxySettings.Proxy;
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
                    LogHelper.Trace(TraceEventType.Error, TraceCategory.Network, errorText);
                }
                throw;
            }
        }

        static void Post(string url, Object obj)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            string jsonContent = JsonConvert.SerializeObject(obj);
            Byte[] byteArray = encoding.GetBytes(jsonContent);
            long length = 0;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                           request.Method = "POST";
                           request.ContentLength = byteArray.Length;
                           request.ContentType = @"application/json";
            request.Proxy = ProxySettings.Proxy;

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
            try
            {
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    length = response.ContentLength;
                }
            }
            catch (WebException ex)
            {
               LogHelper.Trace(TraceEventType.Error, 
                   $"Cmc web api provider send data error: {ex.GetType()} {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }
    }
}
