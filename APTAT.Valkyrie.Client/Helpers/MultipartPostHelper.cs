using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace APTAT.Valkyrie.Client
{
    public static class MultipartPostHelper
    {
        public static string Post(string uploadfile, string url, NameValueCollection payload, string orgFileName, CancellationToken cancellationToken)
        {
            Uri uri = new Uri(url);
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(uri);
            if (ProxySettings.Proxy != null)
            {
                webrequest.Proxy = ProxySettings.Proxy;
            }


            cancellationToken.Register(() =>
            {
                webrequest.Abort();
            });
            webrequest.Timeout = 1000 * 300;
            //   webrequest.ServerCertificateValidationCallback += null;

            string boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            webrequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webrequest.Method = "POST";

            StringBuilder sb = new StringBuilder();
            if (payload != null)
            {
                foreach (string key in payload.Keys)
                {
                    BuildParamHeaderEntry(key, payload.Get(key), boundary, sb);
                }
            }

            if (String.IsNullOrWhiteSpace(orgFileName))
            {
                orgFileName = uploadfile;
            }
            BuildFileUploadHeaderEntry(orgFileName, boundary, sb);

            string postHeader = sb.ToString();
            return SubmitFormData(uploadfile, webrequest, boundary, postHeader);
        }

        private static string SubmitFormData(string uploadfile, HttpWebRequest webrequest, string boundary, string postHeader)
        {
            string serverResponse;
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            using (FileStream fileStream = new FileStream(uploadfile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                long length = postHeaderBytes.Length + fileStream.Length + boundaryBytes.Length;
                webrequest.ContentLength = length;

                Stream requestStream = webrequest.GetRequestStream();

                // Write out our post header
                requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

                // Write out the file contents
                byte[] buffer = new Byte[checked((uint)Math.Min(4096, (int)fileStream.Length))];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    requestStream.Write(buffer, 0, bytesRead);

                // Write out the trailing boundary
                requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                WebResponse responce = webrequest.GetResponse();
                
                Stream s = responce.GetResponseStream();
                using (StreamReader sr = new StreamReader(s))
                {
                    serverResponse = sr.ReadToEnd();
                }
            }

            return serverResponse;
        }

        private static void BuildParamHeaderEntry(string name, string value, string boundary, StringBuilder sb)
        {
            sb.Append("--"); sb.Append(boundary); sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\""); sb.Append(name); sb.Append("\"");
            sb.Append("\r\n");
            sb.Append("\r\n");
            sb.Append(HttpUtility.UrlEncode(value));
            sb.Append("\r\n");
        }

        private static void BuildFileUploadHeaderEntry(string fileName, string boundary, StringBuilder sb)
        {
            sb.Append("--"); sb.Append(boundary); sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\""); sb.Append("file_data"); sb.Append("\"");
            sb.Append("; filename=\"");
            sb.Append(HttpUtility.UrlEncode(fileName.Trim()));
            sb.Append("\"");
            sb.Append("\r\n");
            sb.Append("Content-Type: application / octet - stream");
            sb.Append("\r\n");
            sb.Append("\r\n");
        }
    }
}