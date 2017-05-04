using APTAT.Common;
using APTAT.Core.CamLicense;
using APTAT.Core.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Valkyrie.WebApiProvider;

namespace APTAT.Core
{
    public class CamLicenseProvider : ICamLicenseProvider
    {
        private readonly IMachineKeyProvider _machineKeyProvider;
        private readonly string _secretPhrase = "rogoyhpjtpqbwbqk"; //"dmiuchwodjvedpsh";              
        private readonly string _camUrl = @"https://accounts.comodo.com/signup_service/get_free_license";

        public CamLicenseProvider(IMachineKeyProvider machineKeyProvider)
        {
            _machineKeyProvider = machineKeyProvider;
            if (Settings.Default.UseDevCam)
            {
                _camUrl = @"https://accounts.comodo.od.ua/signup_service/get_free_license";
                _secretPhrase = "rogoyhpjtpqbwbqk";
            }
        }
        public string TryGetFreeLicense(NetworkCredential credential)
        {
            if(credential != null) SetProxy(credential);

            int retry = 0;
            GetFreeLicenseResponse response = new GetFreeLicenseResponse();
            response.Data = new GetFreeLicenseResponseData();
            bool success = false;
            const int interval = 500;
            const int timeout = interval*3;
            int attempts = timeout / interval;
            Exception lastException = null;
            while (!success && retry < attempts)
            {
                try
                {
                    response = GetFreeLicense();
                    success = response.Data.Message == "Success";
                    if (success) return response.Data.LicenseKey;
                    else throw new CamException(response.Data.Message);
                }
                catch (WebException webException)
                {
                    if (webException.Message.Contains("407")) throw;
                    lastException = webException;
                }
                catch (Exception e)
                {
                    lastException = e;
                }
                finally
                {
                    retry++;
                }
                Thread.Sleep(interval);
            }
            if (!success && lastException != null) throw lastException;

            return null;
        }

        private void SetProxy(NetworkCredential networkCredential)
        {
            var proxy = WebRequest.GetSystemWebProxy();
            if (networkCredential != null)
            {
             //   proxy = new WebProxy();
                proxy.Credentials = networkCredential;
            }
            ProxySettings.Proxy = proxy;
        }

        private GetFreeLicenseResponse GetFreeLicense()
        {
            var bytes = GenerateRequestBytes();

            var result = PostData(_camUrl, bytes);
            var serializer = new XmlSerializer(typeof(GetFreeLicenseResponse));
            var serializerErrResponse = new XmlSerializer(typeof(ErrorResponse));
            var reader = new StreamReader(new MemoryStream(result), Encoding.UTF8);

            try
            {
                GetFreeLicenseResponse response = (GetFreeLicenseResponse) serializer.Deserialize(reader);
                return response;
            }
            catch
            {
                var readerErr = new StreamReader(new MemoryStream(result), System.Text.Encoding.UTF8);
                ErrorResponse responseError = (ErrorResponse) serializerErrResponse.Deserialize(readerErr);
                LogHelper.Trace(TraceEventType.Error, TraceCategory.General, _camUrl + " Result Message: " + responseError.Data.Message);
                throw new InvalidOperationException("Could not get CAM license");
            }



        }
        private byte[] GenerateRequestBytes()
        {
            GetFreeLicenseRequest request = new GetFreeLicenseRequest();
            var data = GetData();
            request.Data = data;
            var normilzedContent = GetNormalizedDataContent(data);
            request.Fingerprint = GetFingerprint(normilzedContent);

            var serializer = new XmlSerializer(typeof(GetFreeLicenseRequest));
            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, System.Text.Encoding.UTF8))
                {
                    serializer.Serialize(streamWriter, request);
                    return memoryStream.ToArray();
                }
            }
        }
        private GetFreeLicenseRequestData GetData()
        {
            var data = new GetFreeLicenseRequestData();
            data.AffiliateID = 33220001;
            data.WebApiLogin = "cis_webapi_account";
            data.MachineCode = _machineKeyProvider.GlobalMachineKey;
            data.PricingTermIdentifier = "misc.forensic.analysis.tool.free";
            data.LicenseKeyInfo = new GetFreeLicenseRequestDataLicenseKeyInfo();
            data.LicenseKeyInfo.Version = "1.0.0.0";
            data.LicenseKeyInfo.OsLang = 2052;
            data.LicenseKeyInfo.AppLang = 2052;
            data.LicenseKeyInfo.OsId = 1;
            data.LicenseKeyInfo.OsPlat = 32;
            return data;
        }
        private byte[] PostData(string destinationUrl, byte[] bytes)
        {
            var request = (HttpWebRequest) WebRequest.Create(destinationUrl);
            request.ContentType = "text/xml; encoding='utf-8'";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
            if (ProxySettings.Proxy != null)
            {
                request.Proxy = ProxySettings.Proxy;
            }
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse) request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                using (MemoryStream ms = new MemoryStream())
                {
                    responseStream.CopyTo(ms);
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
            return null;
        }
        private string GetFingerprint(string normalizedData)
        {
            string computedHash = ComputeHash(normalizedData, false);
            //var secretPhrase = "rogoyhpjtpqbwbqk";//"dmiuchwodjvedpsh";
            return ComputeHash(String.Concat(_secretPhrase, "-", computedHash), true);
        }
        private string ComputeHash(string normalizedData, bool toUpper)
        {
            using (var sha1 = new SHA1Managed())
            {
                byte[] data = Encoding.UTF8.GetBytes(normalizedData);
                byte[] hash = sha1.ComputeHash(data);
                string computed = BitConverter.ToString(hash).Replace("-", String.Empty);
                return toUpper ? computed : computed.ToLower();
            }
        }
        private string GetNormalizedDataContent(GetFreeLicenseRequestData data)
        {
            var obj = SerializerHelper<GetFreeLicenseRequestData>.ObjectToXml(data);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(obj);
            doc.RemoveChild(doc.ChildNodes[0]);
            doc.Normalize();
            var normalizedData = doc.OuterXml.Replace("GetFreeLicenseRequestData", "Data");
            return normalizedData;
        }
    }

    public class CamException : Exception
    {
        public CamException(string message):base(message)
        {
            
        }
    }
}