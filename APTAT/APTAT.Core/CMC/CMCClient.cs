using APTAT.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using APTAT.Core.CMC.Request;

namespace APTAT.Core.CMC
{
    public class CmcClient : ICmcClient
    {
        private readonly ICmcWebApiProvider _webAptProvider;
        private string _accountIdentifier;
        private string _sessionIdentifier;
        private string _analyzeIdentifier;
        private readonly string _machineCode;

        private string _productIdentifier;
        private readonly string _applicationIdentifier = "enp";

        private readonly ICultureInfoProvider _cultureInfoProvider;
        private readonly ISystemInfoProvider _systemInfoProvider;
         
        public CmcClient(ICmcWebApiProvider icmcWebApiProvider, IMachineKeyProvider machineKeyProvider,
            ISystemInfoProvider systemInfoProvider, ICultureInfoProvider cultureInfoProvider)
        {
            _webAptProvider = icmcWebApiProvider;
            _machineCode = machineKeyProvider.GlobalMachineKey;
            _systemInfoProvider = systemInfoProvider;
            _cultureInfoProvider = cultureInfoProvider;
          //_productIdentifier = SetProductIdentifier();
        }

        //private string SetProductIdentifier()
        //{
        //    return "enp.c1.fat";
        //}

        private string GetRequestFieldValue(string fieldName)
        {
            List<Parameter> requestFields = new List<Parameter>
            {
                new Parameter() {Value = "acid", Name = "Acid"},
                new Parameter() {Value = "sid", Name = "Sid"},
                new Parameter() {Value = "app_op_sy", Name = "OperationSystemFamilyName"},
                new Parameter() {Value = "app_op_ve", Name = "OperationSystemVersion"},
                new Parameter() {Value = "app_op_pl", Name = "OperationSystemPlatform"},
                new Parameter() {Value = "app_pr_ve", Name = "ProductVersion"},
                new Parameter() {Value = "app_cul", Name = "UICulture"},
                new Parameter() {Value = "iu_tz", Name = "UsersTimeZone"},
                new Parameter() {Value = "iu_ts", Name = "LinuxFormatOfDateTime"},
                new Parameter() {Value = "ds", Name = "DiscoveryStatus"},
                new Parameter() {Value = "pc_cc", Name = "CountOfPotentiallyScanableDiscoveredComputers"},
                new Parameter() {Value = "sc_trg_tp", Name = "ScanTriggerType"},
                new Parameter() {Value = "sc_tg", Name = "ScanTarget"},
                new Parameter() {Value = "sc_tp", Name = "ScanType"},
                new Parameter() {Value = "pc_cc", Name = "TotalCountOfDiscoveredComputers"},
                new Parameter() {Value = "pc_sc", Name = "CountOfScannedComputers"},
                new Parameter() {Value = "u_fc", Name = "CountOfNewUnknownFilesWhichWasFoundAfterTheScanWasFinished"},
                new Parameter()
                {
                    Value = "mlc_fc",
                    Name = "CountOfNewMaliciousFilesWhichWasFoundAfterTheScanWasFinished"
                },
                new Parameter() {Value = "sc_st", Name = "ScanStatus"},
                new Parameter() {Value = "ps_sc_s", Name = "CountOfSuccessScans"},
                new Parameter() {Value = "ps_sc_f", Name = "CountOfFailedScans"},
                new Parameter() {Value = "u_fc", Name = "CountOfUnknownFiles"},
                new Parameter() {Value = "mlc_fc", Name = "CountOfMaliciousFiles"},
                new Parameter() {Value = "rp_kd", Name = "ReportKind"},
                new Parameter() {Value = "act", Name = "ReportAction"}
            };

            string findedfieldValue = string.Empty;
            var requestedField=requestFields.FirstOrDefault(c => c.Name == fieldName);
            if (requestedField != null)
                findedfieldValue= requestedField.Value;
            return findedfieldValue;
        }

        private Dictionary<string, string> GetCmcRequestFieldsAndValues(IRequest request){
            Dictionary<string, string> requestFields = new Dictionary<string, string>();
            var requestProperties = request.GetType().GetProperties();
            foreach (PropertyInfo prop in requestProperties)
            {
                var fieldValue = prop.GetValue(request);
                if (prop.PropertyType.IsEnum)
                    fieldValue = (int) fieldValue;
                var fieldName=GetRequestFieldValue(prop.Name);
                if(fieldName!=String.Empty)
                    requestFields.Add(fieldName, fieldValue?.ToString());
            }
            return requestFields;
        }

        private GeneralInfo GenerateRequest(
            string sessionIdentifier,string analizeIdentifier,string accountIdentifier,
            string eventParamValue,string eventId,string eventGroupId,
            List<Parameter> additionalParameters,IRequest request) {

            if (request == null) return null;
            var requestFieldsAndValues = GetCmcRequestFieldsAndValues(request);

            if (accountIdentifier != null) requestFieldsAndValues["acid"] =accountIdentifier;
            if (sessionIdentifier != null) requestFieldsAndValues["sid"] = sessionIdentifier;
            if (analizeIdentifier!=null)   requestFieldsAndValues["aid"] = analizeIdentifier;
             
            GeneralInfo generalInfo = 
            new GeneralInfo(_applicationIdentifier, _machineCode, _machineCode, _productIdentifier);

            List <Parameter> applicationValues=new List<Parameter>();
            var applicationParameters = additionalParameters.Where(c => c.IsApplication);
            applicationValues.AddRange(applicationParameters);
            generalInfo.Values = applicationValues;

            EventInfo eventInfo = new EventInfo
            {
                EventGroupIdentifier = eventGroupId,
                EventIdentifier = eventId
            };

            List<Parameter> eventValues = new List<Parameter>();
            Parameter eventParam = new Parameter() { Name = "ev_for_hashing", Value = eventParamValue };
            eventValues.Add(eventParam);
            foreach (var evtField in requestFieldsAndValues)
            eventValues.Add(new Parameter(){Name = evtField.Key,Value = evtField.Value});

            var eventsParameters = additionalParameters.Where(c => !c.IsApplication);
            eventValues.AddRange(eventsParameters);

            eventInfo.EventValues = eventValues;
            generalInfo.EventsInfo.Add(eventInfo);

            return generalInfo;
        }

        private string GetLinuxFormatTime(DateTime localTime)
        {
            DateTime unixEpoch= new DateTime(1970, 1, 1);
            TimeSpan timeSpan = localTime - unixEpoch;
            return ((int)timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        public void Launched(ILaunchedRequest request)
        {
            _accountIdentifier = request.Acid;
            _sessionIdentifier = Guid.NewGuid().ToString();
            var systemInfo = _systemInfoProvider.GetSystemInfo();
            var culture = _cultureInfoProvider.GetCulture();
            var localTimeZone = TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now);
            string eventParamValue = "eh_pu";
            string eventId = "ei_da";
            string eventGroupId = "eg_da";
            _productIdentifier = request.ProductIdentifier;
            List<Parameter> additionalParameters =new List<Parameter>()
            {
               new Parameter() { Name = "op_sy", Value = systemInfo.FamilyName,IsApplication = true},
               new Parameter() { Name = "op_ve", Value = systemInfo.Version,IsApplication = true },
               new Parameter() { Name = "op_pl", Value = systemInfo.Platform,IsApplication = true},
               new Parameter() { Name = "cul",   Value =culture, IsApplication = true},
               new Parameter() { Name = "pr_ve", Value = request.ProductVersion ,IsApplication = true},
               new Parameter() { Name = "iu_tz", Value = localTimeZone.ToString(CultureInfo.InvariantCulture)  },
               new Parameter() { Name = "iu_ts", Value =GetLinuxFormatTime(localTimeZone) }
            };

            GeneralInfo generatedrequest = 
                GenerateRequest(_sessionIdentifier,_analyzeIdentifier, _accountIdentifier,
                    eventParamValue, eventId, eventGroupId, additionalParameters, request);

            _webAptProvider.SendData(generatedrequest);
        }

        public void AutoDiscoveryFinished(IAutoDiscoveryFinishedRequest request)
        {
            string eventParamValue = "eh_ad_adf";
            string eventId = "ei_rp_ad";
            string eventGroupId = "eg_da";
            List<Parameter> additionalParameters = new List<Parameter>();

            GeneralInfo generatedrequest = 
                GenerateRequest(_sessionIdentifier, _analyzeIdentifier, _accountIdentifier,
                    eventParamValue, eventId, eventGroupId, additionalParameters, request);

            _webAptProvider.SendData(generatedrequest);
        }

        public void ScanStarted(IScanStartedRequest request)
        {
            string eventParamValue = "eh_sc_ss";
            string eventId = "ei_rp_sc";
            string eventGroupId = "eg_da";

            List<Parameter> additionalParameters = new List<Parameter>();
            _analyzeIdentifier = Guid.NewGuid().ToString();

            GeneralInfo generatedrequest = 
                GenerateRequest(_sessionIdentifier, _analyzeIdentifier, _accountIdentifier,
                    eventParamValue, eventId, eventGroupId, additionalParameters, request);

            _webAptProvider.SendData(generatedrequest);
        }

        public void ScanFinished(IScanFinishedRequest request)
        {
            string eventParamValue = "eh_fat_sf";
            string eventId = "ei_rp_sc";
            string eventGroupId = "eg_da";

            List<Parameter> additionalParameters = new List<Parameter>();
            GeneralInfo generatedrequest =
                GenerateRequest(_sessionIdentifier, _analyzeIdentifier, _accountIdentifier,
                eventParamValue, eventId, eventGroupId, additionalParameters, request);

            _webAptProvider.SendData(generatedrequest);
        }

        public void ScanUpdated(IScanUpdatedRequest request)
        {
            string eventParamValue = "eh_sc_su";
            string eventId = "ei_rp_sc";
            string eventGroupId = "eg_da"; 

            List<Parameter> additionalParameters = new List<Parameter>();
            GeneralInfo generatedrequest =
                GenerateRequest(_sessionIdentifier, _analyzeIdentifier, _accountIdentifier,
                    eventParamValue, eventId, eventGroupId, additionalParameters, request);
             
            _webAptProvider.SendData(generatedrequest);
        }

        public void ReportGenerated(IReportGeneratedRequest request)
        {
            string eventParamValue = "eh_rp_gen";
            string eventId = "ei_rp_info";
            string eventGroupId = "eg_da";

            List<Parameter> additionalParameters = new List<Parameter>();
            GeneralInfo generatedrequest = 
                GenerateRequest(_sessionIdentifier, _analyzeIdentifier, _accountIdentifier,
                    eventParamValue, eventId, eventGroupId, additionalParameters, request);

            _webAptProvider.SendData(generatedrequest);
        }

        public void ReportPrintedOrSaved(IReportPrintedOrSavedRequest request)
        {
            string eventParamValue = "eh_rp_svd";
            string eventId = "ei_rp_info";
            string eventGroupId = "eg_da";

            List<Parameter> additionalParameters = new List<Parameter>();
            GeneralInfo generatedrequest = 
                GenerateRequest(_sessionIdentifier, _analyzeIdentifier, _accountIdentifier,
                    eventParamValue, eventId, eventGroupId, additionalParameters, request);

            _webAptProvider.SendData(generatedrequest);
        }

        public void AppicationClosed(IApplicationClosedRequest request)
        {
            string eventParamValue = "eh_pc";
            string eventId = "ei_da";
            string eventGroupId = "eg_da"; 
              
            List<Parameter> additionalParameters = new List<Parameter>();
            GeneralInfo generatedrequest =
                GenerateRequest(_sessionIdentifier, _analyzeIdentifier, _accountIdentifier,
                    eventParamValue, eventId, eventGroupId, additionalParameters, request);

            _webAptProvider.SendData(generatedrequest);
        }
    }
}
