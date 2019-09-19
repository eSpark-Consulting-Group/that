using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.COT.Login
{
    public class ValkiryeLicenseIDCheckResult
    {
        public string api_key { get; set; }
        public int return_code { get; set; }
        public string result_description { get; set; }
        public string result_message { get; set; }
    }

    public class Params
    {
        public List<object> modules { get; set; }
    }

    public class StartingProcessList
    {
        public string name { get; set; }
        public bool blocking { get; set; }
        public int status { get; set; }
        public object values { get; set; }
        public Params @params { get; set; }
    }

    public class LicenseKey
    {
        public string @namespace { get; set; }
        public bool isAddedToSession { get; set; }
        public bool isDomeSWG { get; set; }
        public List<object> licenseKeys { get; set; }
    }

    public class SessionData
    {
        public string userId { get; set; }
        public string role { get; set; }
        public string email { get; set; }
        public string displayName { get; set; }
        public bool authorized { get; set; }
        public List<string> modules { get; set; }
        public List<string> provisioningModules { get; set; }
        public List<object> suspendedByBillModules { get; set; }
        public List<object> inactivatedByBillModules { get; set; }
        public List<object> suspendedByCamModules { get; set; }
        public List<object> inactivatedByCamModules { get; set; }
        public List<StartingProcessList> startingProcessList { get; set; }
        public string mspId { get; set; }
        public string mspName { get; set; }
        public string mspSubdomain { get; set; }
        public List<string> mspTypes { get; set; }
        public string mspContactEmail { get; set; }
        public string camLogin { get; set; }
        public bool hideRmm { get; set; }
        public string plainPassword { get; set; }
        public string phoneNumber { get; set; }
        public string accountAdminCamLogin { get; set; }
        public List<LicenseKey> licenseKeys { get; set; }
        public string entityType { get; set; }
        public bool azureEnable { get; set; }
        public string userOtpStatus { get; set; }
        public string userOtpConfigStatus { get; set; }
        public string browserStatus { get; set; }
        public string userPassed2FA { get; set; }
    }

    public class Data
    {
        public string token { get; set; }
        public SessionData sessionData { get; set; }
    }

    public class LoginRootObject
    {
        public string code { get; set; }
        public Data data { get; set; }
    }

    public class OtpData
    {
        public string otpKey { get; set; }
    }

    public class LoginOtpObject
    {
        public string code { get; set; }
        public OtpData data { get; set; }
    }
}
public class CheckRootObject
{
    public string status { get; set; }
    public string code { get; set; }
    public string message { get; set; }
    public object data { get; set; }
}