using APTAT.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using APTAT.Core.CMC.Request;
using APTAT.Core.CMC.UFH;
using Valkyrie.WebApiProvider;

namespace APTAT.Core.CMC
{
    public class CmcClient : ICmcClient
    {
        private readonly ICmcWebApiProvider _webApiProvider;
        protected string _accountIdentifier;
        protected string _sessionIdentifier;
        protected string _analyzeIdentifier;

        private readonly string _applicationIdentifier = "enp";
        private readonly ICultureInfoProvider _cultureInfoProvider;
        private readonly ISystemInfoProvider _systemInfoProvider;

        private readonly GeneralInfoParameters _generalInfoParameters;

        public CmcClient(ICmcWebApiProvider icmcWebApiProvider, IMachineKeyProvider machineKeyProvider,
            ISystemInfoProvider systemInfoProvider, ICultureInfoProvider cultureInfoProvider)
        {
            _webApiProvider = icmcWebApiProvider;
            _systemInfoProvider = systemInfoProvider;
            _cultureInfoProvider = cultureInfoProvider;

            _generalInfoParameters = new GeneralInfoParameters
            {
                GlobalMachineCode = machineKeyProvider.GlobalMachineKey,
                MachineCode = machineKeyProvider.GlobalMachineKey,
                ApplicationIdentifier = _applicationIdentifier
            };
        }

        private static string GetLinuxFormatTime(DateTime localTime)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1);
            TimeSpan timeSpan = localTime - unixEpoch;
            return ((int)timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        protected void SendData(IRequest request, EventParams eventParams)
        {
            RequestParameters requestParameters = new RequestParameters()
            {
                SessionIdentifier = _sessionIdentifier,
                AnalizeIdentifier = _analyzeIdentifier,
                AccountIdentifier = _accountIdentifier,

                EventParamValue = eventParams.EventParamValue,
                EventId = eventParams.EventId,
                EventGroupId = eventParams.EventGroupId
            };
            RequestBuilder requestBuilder = new RequestBuilder(_generalInfoParameters);
            var info = requestBuilder.GenerateRequest(requestParameters, eventParams.AdditionalParameters, request);
            _webApiProvider.SendData(info, ProxySettings.Proxy?.Credentials);
        }

        private static string GetTimeZoneFormated()
        {            
            TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            string timeZoneFormated = String.Empty;
            if (offset.Ticks > 0)
            {
                timeZoneFormated = "GMT+" + offset.ToString().Substring(0, 5);
            }
            else
            {
                timeZoneFormated = "GMT" + offset.ToString().Substring(0, 6);
            }
            return timeZoneFormated;
        }

        public void ApplicationLaunched(IApplicationLaunchedRequest request)
        {
            _accountIdentifier = request.AcId;
            _sessionIdentifier = Guid.NewGuid().ToString();
            _generalInfoParameters.ProductIdentifier = request.ProductIdentifier;

            var systemInfo = _systemInfoProvider.GetSystemInfo();
            var culture = _cultureInfoProvider.GetCulture();
            var localTimeZone = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now);
            var timeZoneFormated = GetTimeZoneFormated();

            List<IParameter> additionalParameters = new List<IParameter>()
            {
               new Parameter() { Name = "op_sy", Value = "Windows",IsApplication = true},
               new Parameter() { Name = "op_ve", Value = systemInfo.Version,IsApplication = true },
               new Parameter() { Name = "op_pl", Value = systemInfo.Platform,IsApplication = true},
               new Parameter() { Name = "cul",   Value =culture, IsApplication = true},
               new Parameter() { Name = "pr_ve", Value = request.ProductVersion ,IsApplication = true},
               new Parameter() { Name = "iu_tz", Value = timeZoneFormated },
               new Parameter() { Name = "iu_ts", Value =GetLinuxFormatTime(localTimeZone) }
            };

            var eventParams = GetApplicationLaunchedParams();
            eventParams.AdditionalParameters = additionalParameters;
            SendData(request, eventParams);
        }

        protected virtual EventParams GetApplicationLaunchedParams()
        {
            var eventParams = new EventParams()
            {
                EventId = "ei_da",
                EventParamValue = "eh_pu",
                EventGroupId = "eg_da"
            };
            return eventParams;
        }

        public void AutoDiscoveryFinished(IAutoDiscoveryFinishedRequest request)
        {
            EventParams eventParams = new EventParams()
            {
                EventParamValue = "eh_ad_adf",
                EventId = "ei_rp_ad",
                 EventGroupId = "eg_da",
                AdditionalParameters = new List<IParameter>() { }
            };
            SendData(request, eventParams);
        }

        public void EmailAction(IEmailActionRequest request)
        {

        }

        public void ScanStarted(IScanStartedRequest request)
        {
            _analyzeIdentifier = Guid.NewGuid().ToString();

            EventParams eventParams = GetScanStartedParams();
            SendData(request, eventParams);
        }
        protected virtual EventParams GetScanStartedParams()
        {
            return new EventParams()
            {
                EventParamValue = "eh_sc_ss",
                EventId = "ei_rp_sc",
                EventGroupId = "eg_da",
                AdditionalParameters = new List<IParameter>() { }
            };
        }

        public void ScanUpdated(IScanUpdatedRequest request)
        {
            EventParams eventParams = GetScanUpdatedParams();
            SendData(request, eventParams);
        }
        protected virtual EventParams GetScanUpdatedParams()
        {
            return new EventParams()
            {
                EventParamValue = "eh_sc_su",
                EventId = "ei_rp_sc",
                EventGroupId = "eg_da",
                AdditionalParameters = new List<IParameter>() { }
            };
        }

        public void ScanFinished(IScanFinishedRequest request)
        {
            EventParams eventParams = GetScanFinishedParams();
            SendData(request, eventParams);
        }
        protected virtual EventParams GetScanFinishedParams()
        {
            return new EventParams()
            {
                EventParamValue = "eh_sc_sf",
                EventId = "ei_rp_sc",
                EventGroupId = "eg_da",
                AdditionalParameters = new List<IParameter>() { }
            };
        }

        public void ReportGenerated(IReportGeneratedRequest request)
        {
            EventParams eventParams = GetReportGeneratedParams();
            SendData(request, eventParams);
        }
        protected virtual EventParams GetReportGeneratedParams()
        {
            return new EventParams()
            {
                EventParamValue = "eh_rp_gen",
                EventId = "ei_rp_info",
                EventGroupId = "eg_da",
                AdditionalParameters = new List<IParameter>() { }
            };
        }

        public void ReportPrintedOrSaved(IReportPrintedOrSavedRequest request)
        {
            EventParams eventParams = GetReportPrintedOrSavedParams();
            SendData(request, eventParams);
        }
        protected virtual EventParams GetReportPrintedOrSavedParams()
        {
            return new EventParams()
            {
                EventParamValue = "eh_rp_svd",
                EventId = "ei_rp_info",
                EventGroupId = "eg_da",
                AdditionalParameters = new List<IParameter>() { }
            };
        }

        public void ApplicationClosed(IApplicationClosedRequest request)
        {
            EventParams eventParams = GetApplicationClosedOrSavedParams();
            SendData(request, eventParams);
        }
        protected virtual EventParams GetApplicationClosedOrSavedParams()
        {
            return new EventParams()
            {
                EventParamValue = "eh_pc",
                EventId = "ei_da",
                EventGroupId = "eg_da",
                AdditionalParameters = new List<IParameter>() { }
            };
        }

    }
}
