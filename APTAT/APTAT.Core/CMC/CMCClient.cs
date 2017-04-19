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
        private readonly ICmcWebApiProvider _webApiProvider;
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
            _webApiProvider = icmcWebApiProvider;
            _machineCode = machineKeyProvider.GlobalMachineKey;
            _systemInfoProvider = systemInfoProvider;
            _cultureInfoProvider = cultureInfoProvider;
        }

        private string GetRequestFieldValue(string fieldName)
        {
            List<Parameter> requestFields = new List<Parameter>
            {
                new Parameter() {Value = "acid", Name =nameof(ILaunchedRequest.AcId)},


                new Parameter() {Value = "app_pr_ve", Name =nameof(ILaunchedRequest.ProductVersion)},

                new Parameter() {Value = "ds", Name =nameof(IAutoDiscoveryFinishedRequest.DiscoveryStatus)},
                new Parameter() {Value = "pc_cc", Name =nameof(IAutoDiscoveryFinishedRequest.CountOfPotentiallyScanableDiscoveredComputers)},

                new Parameter() {Value = "sc_trg_tp", Name =nameof(IScanStartedRequest.ScanTriggerType)},
                new Parameter() {Value = "sc_tg", Name =nameof(IScanStartedRequest.ScanTarget)}, 
                new Parameter() {Value = "sc_tp", Name =nameof(IScanStartedRequest.ScanType)}, 
                new Parameter() {Value = "pc_cc", Name =nameof(IScanStartedRequest.TotalCountOfDiscoveredComputers)}, 
                new Parameter() {Value = "pc_sc", Name =nameof(IScanStartedRequest.CountOfScannedComputers)}, 

                new Parameter() {Value = "u_fc", Name = nameof(IScanUpdatedRequest.CountOfNewUnknownFilesWhichWasFoundAfterTheScanWasFinished)},
                new Parameter() {Value = "mlc_fc", Name =nameof(IScanUpdatedRequest.CountOfNewMaliciousFilesWhichWasFoundAfterTheScanWasFinished)},

                new Parameter() {Value = "sc_st", Name =nameof(IScanFinishedRequest.ScanStatus)},
                new Parameter() {Value = "ps_sc_s", Name =nameof(IScanFinishedRequest.CountOfSuccessScans)},
                new Parameter() {Value = "ps_sc_f", Name =nameof(IScanFinishedRequest.CountOfFailedScans)},
                new Parameter() {Value = "u_fc", Name =nameof(IScanFinishedRequest.CountOfUnknownFiles)},
                new Parameter() {Value = "mlc_fc", Name =nameof(IScanFinishedRequest.CountOfMaliciousFiles)}, 

                new Parameter() {Value = "rp_kd", Name =nameof(IReportGeneratedRequest.ReportKind)},
                new Parameter() {Value = "act",Name =nameof(IReportPrintedOrSavedRequest.ReportAction)},

                new Parameter() {Value = "sid", Name =nameof(IApplicationClosedRequest.SId)}
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

        private GeneralInfo GenerateRequest(RequestParameters requestParameters,List<Parameter> additionalParameters, IRequest request)
        {
            if (request == null) return null;
            var requestFieldsAndValues = GetCmcRequestFieldsAndValues(request);

            if (requestParameters.AccountIdentifier != null) requestFieldsAndValues["acid"] = requestParameters.AccountIdentifier;
            if (requestParameters.SessionIdentifier != null) requestFieldsAndValues["sid"] = requestParameters.SessionIdentifier;
            if (requestParameters.AnalizeIdentifier !=null)   requestFieldsAndValues["aid"] = requestParameters.AnalizeIdentifier;
             
            GeneralInfo generalInfo = new GeneralInfo(_applicationIdentifier, _machineCode, _machineCode, _productIdentifier);


            List<Parameter> applicationValues = new List<Parameter>();
            var applicationParameters = additionalParameters.Where(c => c.IsApplication);
            applicationValues.AddRange(applicationParameters);
            generalInfo.Values = applicationValues;

            EventInfo eventInfo = new EventInfo
            {
                EventGroupIdentifier = requestParameters.EventGroupId,
                EventIdentifier = requestParameters.EventId
            };

            List<Parameter> eventValues = new List<Parameter>();
            Parameter eventParam = new Parameter() { Name = "ev_for_hashing", Value = requestParameters.EventParamValue };
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
            _accountIdentifier = request.AcId;
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
            RequestParameters requestParameters=new RequestParameters(){
                SessionIdentifier = _sessionIdentifier,
                AnalizeIdentifier = _analyzeIdentifier,
                AccountIdentifier = _accountIdentifier,
                EventParamValue = eventParamValue,
                EventId = eventId,
                EventGroupId = eventGroupId
            };

            IGeneralInfo generatedrequest = GenerateRequest(requestParameters, additionalParameters, request);
            _webApiProvider.SendData(generatedrequest);
        }

        public void AutoDiscoveryFinished(IAutoDiscoveryFinishedRequest request)
        {
            string eventParamValue = "eh_ad_adf";
            string eventId = "ei_rp_ad";
            string eventGroupId = "eg_da";
            List<Parameter> additionalParameters = new List<Parameter>();

            RequestParameters requestParameters = new RequestParameters()
            {
                SessionIdentifier = _sessionIdentifier,
                AnalizeIdentifier = _analyzeIdentifier,
                AccountIdentifier = _accountIdentifier,
                EventParamValue = eventParamValue,
                EventId = eventId,
                EventGroupId = eventGroupId
            };
            GeneralInfo generatedrequest = GenerateRequest(requestParameters, additionalParameters, request);
            _webApiProvider.SendData(generatedrequest);
        }

        public void ScanStarted(IScanStartedRequest request)
        {
            string eventParamValue = "eh_sc_ss";
            string eventId = "ei_rp_sc";
            string eventGroupId = "eg_da";

            List<Parameter> additionalParameters = new List<Parameter>();
            _analyzeIdentifier = Guid.NewGuid().ToString();

            RequestParameters requestParameters = new RequestParameters()
            {
                SessionIdentifier = _sessionIdentifier,
                AnalizeIdentifier = _analyzeIdentifier,
                AccountIdentifier = _accountIdentifier,
                EventParamValue = eventParamValue,
                EventId = eventId,
                EventGroupId = eventGroupId
            };
            GeneralInfo generatedrequest = GenerateRequest(requestParameters, additionalParameters, request);
            _webApiProvider.SendData(generatedrequest);
        }

        public void ScanFinished(IScanFinishedRequest request)
        {
            string eventParamValue = "eh_fat_sf";
            string eventId = "ei_rp_sc";
            string eventGroupId = "eg_da";

            List<Parameter> additionalParameters = new List<Parameter>();
            RequestParameters requestParameters = new RequestParameters()
            {
                SessionIdentifier = _sessionIdentifier,
                AnalizeIdentifier = _analyzeIdentifier,
                AccountIdentifier = _accountIdentifier,
                EventParamValue = eventParamValue,
                EventId = eventId,
                EventGroupId = eventGroupId
            };
            GeneralInfo generatedrequest = GenerateRequest(requestParameters, additionalParameters, request);
            _webApiProvider.SendData(generatedrequest);
        }

        public void ScanUpdated(IScanUpdatedRequest request)
        {
            string eventParamValue = "eh_sc_su";
            string eventId = "ei_rp_sc";
            string eventGroupId = "eg_da"; 

            List<Parameter> additionalParameters = new List<Parameter>();
            RequestParameters requestParameters = new RequestParameters()
            {
                SessionIdentifier = _sessionIdentifier,
                AnalizeIdentifier = _analyzeIdentifier,
                AccountIdentifier = _accountIdentifier,
                EventParamValue = eventParamValue,
                EventId = eventId,
                EventGroupId = eventGroupId
            };
            GeneralInfo generatedrequest = GenerateRequest(requestParameters, additionalParameters, request);
            _webApiProvider.SendData(generatedrequest);
        }

        public void ReportGenerated(IReportGeneratedRequest request)
        {
            string eventParamValue = "eh_rp_gen";
            string eventId = "ei_rp_info";
            string eventGroupId = "eg_da";

            List<Parameter> additionalParameters = new List<Parameter>();
            RequestParameters requestParameters = new RequestParameters()
            {
                SessionIdentifier = _sessionIdentifier,
                AnalizeIdentifier = _analyzeIdentifier,
                AccountIdentifier = _accountIdentifier,
                EventParamValue = eventParamValue,
                EventId = eventId,
                EventGroupId = eventGroupId
            };
            GeneralInfo generatedrequest = GenerateRequest(requestParameters, additionalParameters, request);
            _webApiProvider.SendData(generatedrequest);
        }

        public void ReportPrintedOrSaved(IReportPrintedOrSavedRequest request)
        {
            string eventParamValue = "eh_rp_svd";
            string eventId = "ei_rp_info";
            string eventGroupId = "eg_da";

            List<Parameter> additionalParameters = new List<Parameter>();
            RequestParameters requestParameters = new RequestParameters()
            {
                SessionIdentifier = _sessionIdentifier,
                AnalizeIdentifier = _analyzeIdentifier,
                AccountIdentifier = _accountIdentifier,
                EventParamValue = eventParamValue,
                EventId = eventId,
                EventGroupId = eventGroupId
            };
            GeneralInfo generatedrequest = GenerateRequest(requestParameters, additionalParameters, request);
            _webApiProvider.SendData(generatedrequest);
        }

        public void AppicationClosed(IApplicationClosedRequest request)
        {
            string eventParamValue = "eh_pc";
            string eventId = "ei_da";
            string eventGroupId = "eg_da"; 
              
            List<Parameter> additionalParameters = new List<Parameter>();
            RequestParameters requestParameters = new RequestParameters()
            {
                SessionIdentifier = _sessionIdentifier,
                AnalizeIdentifier = _analyzeIdentifier,
                AccountIdentifier = _accountIdentifier,
                EventParamValue = eventParamValue,
                EventId = eventId,
                EventGroupId = eventGroupId
            };
            GeneralInfo generatedrequest = GenerateRequest(requestParameters, additionalParameters, request);
            _webApiProvider.SendData(generatedrequest);
        }
    }
}
