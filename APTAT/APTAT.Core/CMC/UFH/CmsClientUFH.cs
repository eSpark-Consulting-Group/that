using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.Core.CMC.Request;

namespace APTAT.Core.CMC.UFH
{
    public class EventParams
    {
        public string EventParamValue { get; set; }
        public string EventId { get; set; }
        public string EventGroupId { get; set; }
        public List<IParameter> AdditionalParameters { get; set; }

    }

    public class CmcClientUfh : CmcClient
    {
        public CmcClientUfh(ICmcWebApiProvider icmcWebApiProvider, IMachineKeyProvider machineKeyProvider, ISystemInfoProvider systemInfoProvider, ICultureInfoProvider cultureInfoProvider) : base(icmcWebApiProvider, machineKeyProvider, systemInfoProvider, cultureInfoProvider)
        {

        }

        protected override EventParams GetScanStartedParams()
        {
            return new EventParams()
            {
                EventParamValue = "eh_sc_ss",
                EventId = "ei_rp_sc",
                EventGroupId = "eg_da",
                AdditionalParameters = new List<IParameter>()
                {

                }
            };
        }


        public void ValkyrieUnkownFileUploadUpdated(IValkyrieUnkownFileUploadWithTrigger request)
        {
            string eventId = "ei_rp_vuu";
            string eventParamValue = "eh_ufh_vuu";
            string eventGroupId = "eg_da";
            List<Parameter> additionalParameters = new List<Parameter>
            {
                new Parameter
                {
                    IsApplication = false,
                    Name = "vuu_tfc",
                    Value = request.TotalFilesCount.ToString()
                },
                new Parameter
                {
                    IsApplication = false,
                    Name = "vuu_mfc",
                    Value = request.MalwareFilesCount.ToString()
                },
                new Parameter
                {
                    IsApplication = false,
                    Name = "vu_trg",
                    Value = ((int)request.UploadTrigger).ToString()

                }
            };


            RequestParameters requestParameters = new RequestParameters()
            {
                SessionIdentifier = _sessionIdentifier,
                AnalizeIdentifier = _analyzeIdentifier,
                AccountIdentifier = _accountIdentifier,
                EventParamValue = eventParamValue,
                EventId = eventId,
                EventGroupId = eventGroupId
            };

         //   SendData(requestParameters, additionalParameters, request);
        }

        public void ValkyrieUnkownFileUpload(IValkyrieUnkownFileUpload request)
        {
            string eventId = "ei_rp_vu";
            string eventParamValue = "eh_ufh_vu";
            string eventGroupId = "eg_da";
            List<Parameter> additionalParameters = new List<Parameter>
            {
                new Parameter
                {
                    IsApplication = false,
                    Name = "vuu_tfc",
                    Value = request.TotalFilesCount.ToString()
                },
                new Parameter
                {
                    IsApplication = false,
                    Name = "vuu_mfc",
                    Value = request.MalwareFilesCount.ToString()
                }
            };


            RequestParameters requestParameters = new RequestParameters()
            {
                SessionIdentifier = _sessionIdentifier,
                AnalizeIdentifier = _analyzeIdentifier,
                AccountIdentifier = _accountIdentifier,
                EventParamValue = eventParamValue,
                EventId = eventId,
                EventGroupId = eventGroupId
            };

           // SendData(requestParameters, additionalParameters, request);
        }

    }
}
