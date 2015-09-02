using System;
using System.Configuration;

namespace APTAT.Valkyrie.Client
{
    public static class ValkyrieApiUrls
    {
        private const string basicInfo = "fvs_basic_info";
        private const string fullInfo = "fvs_full_info";
        private const string submitToManual = "fvs_submit_manual";
        private const string submitToAuto = "fvs_submit_auto";
        private const string manualStatus = "fvs_manual_status";
        private const string registerWithMachineID = "fvs_register_with_machine_id";
        
        public static Uri BasicInfo { get; private set; }
        public static Uri FullInfo { get; private set; }
        public static Uri SubmitToManualAnalysis { get; private set; }
        public static Uri SubmitToAutomaticAnalysis { get; private set; }
        public static Uri ManualStatus { get; private set; }
        public static Uri RegisterWithMachineId { get; private set; }

        static ValkyrieApiUrls()
        {
            var baseUri = GetBaseUri();
            BasicInfo = new Uri(baseUri, basicInfo);
            FullInfo = new Uri(baseUri, fullInfo);
            SubmitToManualAnalysis = new Uri(baseUri, submitToManual);
            SubmitToAutomaticAnalysis = new Uri(baseUri, submitToAuto);
            ManualStatus = new Uri(baseUri, manualStatus);
            RegisterWithMachineId = new Uri(baseUri, registerWithMachineID);
        }
        public static Uri GetBaseUri()
        {
            var url = ConfigurationManager.AppSettings["valkyrieBaseUrl"];
            var strUrl = String.Format(url);
            return new Uri(strUrl);
        }
    }
}
