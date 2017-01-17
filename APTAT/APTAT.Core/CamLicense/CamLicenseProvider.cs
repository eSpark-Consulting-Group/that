using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace APTAT.Core
{
    public class CamLicenseProvider
    {
        private const string secretPhrase = "rogoyhpjtpqbwbqk"; //"dmiuchwodjvedpsh";
        //private const string secretPhrase = "dmiuchwodjvedpsh";
        private const string camUrl = @"https://accounts.comodo.od.ua/signup_service/get_free_license";
        //private const string camUrl = @"https://accounts.comodo.com/signup_service/get_free_license";

        public string TryGetFreeLicense()
        {
       //     throw new InvalidOperationException("Test ...");
            int retry = 0;
            GetFreeLicenseResponse response = new GetFreeLicenseResponse();
            response.Data = new GetFreeLicenseResponseData();
            bool success = false;
            const int Interval = 10000;
            const int Timeout = 180000;
            int attempts = Timeout / Interval;

            while (!success && retry < attempts)
            {
                try
                {
                    response = GetFreeLicense();
                    success = response.Data.Message == "Success";
                    if (success) return response.Data.PackageAdditionalParams.PackageItem.LicenseKey;
                }
                catch (Exception e)
                {
                    retry++;
                }
                Thread.Sleep(Interval);
            }
            return null;
        }

        public GetFreeLicenseResponse GetFreeLicense()
        {
            var bytes = GenerateRequestBytes();

            var result = PostData(camUrl, bytes);
            File.WriteAllBytes(Path.GetFileName(Path.GetTempFileName() + ".xml"), bytes);
            File.WriteAllBytes(Path.GetFileName(Path.GetTempFileName() + ".res.xml"), result);

            string resultStr = System.Text.Encoding.UTF8.GetString(result);
            var serializer = new XmlSerializer(typeof(GetFreeLicenseResponse));
            var serializerErrResponse = new XmlSerializer(typeof(ErrorResponse));
            var reader = new StreamReader(new MemoryStream(result), System.Text.Encoding.UTF8);
        
            try
            {                
                GetFreeLicenseResponse response = (GetFreeLicenseResponse) serializer.Deserialize(reader);
                return response;                
            }
            catch 
            {
                var readerErr = new StreamReader(new MemoryStream(result), System.Text.Encoding.UTF8);
                ErrorResponse responseError = (ErrorResponse) serializerErrResponse.Deserialize(readerErr);
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
            data.MachineCode = MachineKey.CryptedMachineKey;
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
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            request.ContentType = "text/xml; encoding='utf-8'";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
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
            return ComputeHash(String.Concat(secretPhrase, "-", computedHash), true);
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
}