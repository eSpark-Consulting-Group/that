using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace APTAT.Valkyrie.Client
{
    public static class MultipartPostHelper
    {
        public static string Post(string uploadfile, string url, NameValueCollection payload, string fileFormFieldName = "file_data")
        {
            Uri uri = new Uri(url);
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(uri);
            webrequest.Timeout = 1000 * 180;
            webrequest.ServerCertificateValidationCallback += null;
                        
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

            BuildFileUploadHeaderEntry(uploadfile, fileFormFieldName, boundary, sb);

            string postHeader = sb.ToString();
            return SubmitFormData(uploadfile, webrequest, boundary, postHeader);
        }

        private static string SubmitFormData(string uploadfile, HttpWebRequest webrequest, string boundary, string postHeader)
        {
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            FileStream fileStream = new FileStream(uploadfile, FileMode.Open, FileAccess.Read);
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
            StreamReader sr = new StreamReader(s);

            return sr.ReadToEnd();
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
        private static void BuildFileUploadHeaderEntry(string uploadfile, string fileFormName, string boundary, StringBuilder sb)
        {
            sb.Append("--"); sb.Append(boundary); sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\""); sb.Append(fileFormName); sb.Append("\"");

            sb.Append("; filename=\"");
            var filePathEncoded = HttpUtility.UrlEncode(Path.GetFileName(uploadfile));
            sb.Append(filePathEncoded);
            sb.Append("\"");
            sb.Append("\r\n");
            sb.Append("Content-Type: application / octet - stream");
            sb.Append("\r\n");
            sb.Append("\r\n");
        }
    }
}