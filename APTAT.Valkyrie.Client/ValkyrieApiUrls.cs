using System;
using System.Configuration;

namespace Valkyrie.WebApiProvider
{
    public static class ValkyrieApiUrls
    {
        public static Uri BasicInfo { get; private set; }
        public static Uri FullInfo { get; private set; }
        public static Uri SubmitToManualAnalysis { get; private set; }
        public static Uri SubmitToManualAnalysisWithoutFileUpload { get; private set; }
        public static Uri SubmitToAutomaticAnalysis { get; private set; }
        public static Uri ManualStatus { get; private set; }
        public static Uri RegisterWithMachineId { get; private set; }
        public static Uri FinalizeScan { get; private set; }
        public static Uri StartScan { get; private set; }
        public static Uri EndScan { get; private set; }
        public static Uri RegisterWithCamLicense { get; private set; }
        public static Uri RegisterWithCamCredentials { get; private set; }
        public static Uri CheckAnalysisResult { get; private set; } 

    static ValkyrieApiUrls()
        {
            var baseUri = GetBaseUri();
            BasicInfo = new Uri(baseUri, "fvs_basic_info");
            FullInfo = new Uri(baseUri, "fvs_full_info");
            SubmitToManualAnalysis = new Uri(baseUri, "fvs_submit_manual");
            SubmitToManualAnalysisWithoutFileUpload = new Uri(baseUri, "fvs_submit_manual_sha1");
            SubmitToAutomaticAnalysis = new Uri(baseUri, "fvs_submit_auto");
            ManualStatus = new Uri(baseUri, "fvs_manual_status");
            RegisterWithMachineId = new Uri(baseUri, "fvs_register_with_machine_id");
            FinalizeScan = new Uri(baseUri, "apt_finalize_scan");
            StartScan = new Uri(baseUri, "apt_start_scan");
            EndScan = new Uri(baseUri, "apt_end_scan");
            RegisterWithCamLicense = new Uri(baseUri, "fvs_register_with_cam_license");
            RegisterWithCamCredentials = new Uri(baseUri, "fvs_register_with_cam_credentials");
            CheckAnalysisResult = new Uri(baseUri, "fvs_check_analysis_result");
        }
        
        public static Uri GetBaseUri()
        {
            var url = ConfigurationManager.AppSettings["valkyrieBaseUrl"];
            var strUrl = String.Format(url);
            return new Uri(strUrl);
        }

        public static bool IsProduction
        {
            get
            {
                var url = GetBaseUri();
                return url.ToString().Contains(("valkyrie.comodo.com"));
            }
        }

    }
}
