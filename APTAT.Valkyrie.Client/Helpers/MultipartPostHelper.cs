using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using APTAT.Valkyrie.Client;

namespace Valkyrie.WebApiProvider.Helpers
{
    internal static class MultipartPostHelper
    {

        internal static string Post(FileStream fileStream, string url, NameValueCollection payload, string fileName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(fileName))
            {
                throw new InvalidOperationException("file name is required");
            }
            return SubmitForm(url, fileStream, fileName, payload, cancellationToken);
        }

        internal static string Post(byte[] uploadfileBytes, string url, NameValueCollection payload, string fileName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(fileName))
            {
                throw new InvalidOperationException("file name is required");
            }

            using (MemoryStream fileStream = new MemoryStream(uploadfileBytes))
            {
                return SubmitForm(url, fileStream, fileName, payload, cancellationToken);
            }
        }


        internal static string Post(string uploadfilePath, string url, NameValueCollection payload, string orgFileName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(orgFileName))
            {
                orgFileName = uploadfilePath;
            }

            using (FileStream fileStream = new FileStream(uploadfilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return SubmitForm(url, fileStream,  orgFileName, payload,  cancellationToken);
            }
        }
      

        private static string SubmitForm(string url, Stream stream, String fileName, NameValueCollection payload,  CancellationToken cancellationToken)
        {
            string boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");

            string postHeader = BuildHeaders(payload, fileName, boundary);

            HttpWebRequest webrequest = CreateWebRequest(url, boundary, cancellationToken);
            string serverResponse;
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);
            byte[] boundaryBytes = Encoding.ASCII.GetBytes(string.Format("\r\n--{0}--\r\n", boundary));

            long length = postHeaderBytes.Length + stream.Length + boundaryBytes.Length;
            webrequest.ContentLength = length;

            Stream requestStream = webrequest.GetRequestStream();

            // Write out our post header
            requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

            // Write out the file contents
            byte[] buffer = new Byte[checked((uint)Math.Min(4096, (int)stream.Length))];
            int bytesRead = 0;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                requestStream.Write(buffer, 0, bytesRead);

            // Write out the trailing boundary
            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            WebResponse responce = webrequest.GetResponse();

            Stream s = responce.GetResponseStream();
            using (StreamReader sr = new StreamReader(s))
            {
                serverResponse = sr.ReadToEnd();
            }

            return serverResponse;
        }

        private static string BuildHeaders(NameValueCollection payload, string fileName, string boundary)
        {
            StringBuilder sb = new StringBuilder();
            if (payload != null)
            {
                foreach (string key in payload.Keys)
                {
                    BuildParamHeaderEntry(key, payload.Get(key), boundary, sb);
                }
            }

            BuildFileUploadHeaderEntry(fileName, boundary, sb);

            string postHeader = sb.ToString();

            return postHeader;
        }

        private static void BuildParamHeaderEntry(string name, string value, string boundary, StringBuilder sb)
        {
            sb.Append("--"); sb.Append(boundary); sb.Append(Environment.NewLine);
            sb.Append("Content-Disposition: form-data; name=\""); sb.Append(name); sb.Append("\"");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(HttpUtility.UrlEncode(value));
            sb.Append(Environment.NewLine);
        }

        private static void BuildFileUploadHeaderEntry(string fileName, string boundary, StringBuilder sb)
        {
            sb.Append("--"); sb.Append(boundary); sb.Append(Environment.NewLine);
            sb.Append("Content-Disposition: form-data; name=\""); sb.Append("file_data"); sb.Append("\"");
            sb.Append("; filename=\"");
            sb.Append(HttpUtility.UrlEncode(fileName.Trim()));
            sb.Append("\"");
            sb.Append(Environment.NewLine);
            sb.Append("Content-Type: application / octet - stream");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
        }

        private static HttpWebRequest CreateWebRequest(string url, string boundary, CancellationToken cancellationToken)
        {
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
            if (ProxySettings.Proxy != null)
            {
                webrequest.Proxy = ProxySettings.Proxy;
            }

            if (cancellationToken != null)
            {
                cancellationToken.Register(() =>
                {
                    webrequest.Abort();
                });
            }
            webrequest.Timeout = 1000 * 300;

            webrequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webrequest.Method = "POST";
            return webrequest;
        }
    }
}