using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using APTAT.Core.CMC.Request;

namespace APTAT.Core.CMC
{
    public class RequestBuilder
    {
        private readonly IGeneralInfoParameters _generalInfoParameters;

        readonly List<Parameter> _requestFields = new List<Parameter>
        {
            new Parameter() {Value = "acid", Name =nameof(IApplicationLaunchedRequest.AcId)},
           

            new Parameter() {Value = "ds", Name =nameof(IAutoDiscoveryFinishedRequest.DiscoveryStatus)},
            new Parameter() {Value = "pc_cc", Name =nameof(IAutoDiscoveryFinishedRequest.CountOfPotentiallyScanableDiscoveredComputers)},

            new Parameter() {Value = "sc_trg_tp", Name =nameof(IScanStartedRequest.ScanTriggerType)},
            new Parameter() {Value = "sc_tg", Name =nameof(IScanStartedRequest.ScanTarget)},
            new Parameter() {Value = "sc_tp", Name =nameof(IScanStartedRequest.ScanType)},
            new Parameter() {Value = "pc_cc", Name =nameof(IScanStartedRequest.TotalCountOfDiscoveredComputers)},
            new Parameter() {Value = "pc_sc", Name =nameof(IScanStartedRequest.CountOfScannedComputers)},

            new Parameter() {Value = "u_fc", Name = nameof(IScanUpdatedRequest.CountOfNewUnknownFilesAfterTheScanWasFinished)},
            new Parameter() {Value = "mlc_fc", Name =nameof(IScanUpdatedRequest.CountOfNewMaliciousFilesAfterTheScanWasFinished)},

            new Parameter() {Value = "sc_st", Name =nameof(IScanFinishedRequest.ScanStatus)},
            new Parameter() {Value = "pc_sc_s", Name =nameof(IScanFinishedRequest.CountOfSuccessScans)},
            new Parameter() {Value = "pc_sc_f", Name =nameof(IScanFinishedRequest.CountOfFailedScans)},
            new Parameter() {Value = "u_fc", Name =nameof(IScanFinishedRequest.CountOfUnknownFiles)},
            new Parameter() {Value = "mlc_fc", Name =nameof(IScanFinishedRequest.CountOfMaliciousFiles)},

            new Parameter() {Value = "rp_kd", Name =nameof(IReportGeneratedRequest.ReportKind)},
            new Parameter() {Value = "act",Name =nameof(IReportPrintedOrSavedRequest.ReportAction)},
        };


        public RequestBuilder(IGeneralInfoParameters generalInfoParameters)
        {
            _generalInfoParameters = generalInfoParameters;
        }

        private string GetRequestFieldValue(string fieldName)
        {
            string findedfieldValue = string.Empty;
            var requestedField = _requestFields.FirstOrDefault(c => c.Name == fieldName);
            if (requestedField != null)
                findedfieldValue = requestedField.Value;
            return findedfieldValue;
        }

        private Dictionary<string, string> GetCmcRequestFieldsAndValues(IRequest request)
        {
            Dictionary<string, string> requestFields = new Dictionary<string, string>();
            var requestProperties = request.GetType().GetProperties();
            foreach (PropertyInfo prop in requestProperties)
            {
                var fieldValue = prop.GetValue(request);
                if (prop.PropertyType.IsEnum)
                    fieldValue = (int)fieldValue;
                var fieldName = GetRequestFieldValue(prop.Name);
                if (fieldName != String.Empty && fieldValue != null)
                    requestFields.Add(fieldName, fieldValue?.ToString());
            }
            return requestFields;
        }

        public GeneralInfo GenerateRequest(IRequestParameters requestParameters, 
            List<IParameter> additionalParameters, IRequest request)
        {
            if (request == null) return null;
            var requestFieldsAndValues = GetCmcRequestFieldsAndValues(request);

            if (requestParameters.AccountIdentifier != null) requestFieldsAndValues["acid"] = requestParameters.AccountIdentifier;
            if (requestParameters.SessionIdentifier != null) requestFieldsAndValues["sid"] = requestParameters.SessionIdentifier;
            if (requestParameters.AnalizeIdentifier != null) requestFieldsAndValues["aid"] = requestParameters.AnalizeIdentifier;

            GeneralInfo generalInfo = new GeneralInfo(
                _generalInfoParameters.ApplicationIdentifier,
                _generalInfoParameters.MachineCode,
                _generalInfoParameters.GlobalMachineCode,
                _generalInfoParameters.ProductIdentifier);

            List<IParameter> applicationValues = new List<IParameter>();
            var applicationParameters = additionalParameters.Where(c => c.IsApplication);
            applicationValues.AddRange(applicationParameters);
            generalInfo.Values = applicationValues;

            EventInfo eventInfo = new EventInfo
            {
                EventGroupIdentifier = requestParameters.EventGroupId,
                EventIdentifier = requestParameters.EventId
            };

            List<IParameter> eventValues = new List<IParameter>();
            Parameter eventParam = new Parameter() { Name = "ev_for_hashing", Value = requestParameters.EventParamValue };
            eventValues.Add(eventParam);
            foreach (var evtField in requestFieldsAndValues)
                eventValues.Add(new Parameter() { Name = evtField.Key, Value = evtField.Value });

            var eventsParameters = additionalParameters.Where(c => !c.IsApplication);
            eventValues.AddRange(eventsParameters);
            eventInfo.EventValues = eventValues;
            generalInfo.EventsInfo.Add(eventInfo);
            return generalInfo;
        }
    }
}