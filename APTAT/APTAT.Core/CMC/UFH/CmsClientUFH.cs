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

    public interface ICmcClientUfh
    {
        void ValkyrieUnkownFileUpload(IValkyrieUnkownFileUploadWithTrigger request);
        void ValkyrieUnkownFileUploadUpdated(IValkyrieUnkownFileUpload request);
        void ApplicationLaunched(IApplicationLaunchedRequest request);
        void AutoDiscoveryFinished(IAutoDiscoveryFinishedRequest request);
        void ScanStarted(IScanStartedRequest request);
        void ScanUpdated(IScanUpdatedRequest request);
        void ScanFinished(IScanFinishedRequest request);
        void ReportGenerated(IReportGeneratedRequest request);
        void ReportPrintedOrSaved(IReportPrintedOrSavedRequest request);
        void ApplicationClosed(IApplicationClosedRequest request);
    }

    public class CmcClientUfh : CmcClient, ICmcClientUfh
    {
        public CmcClientUfh(ICmcWebApiProvider icmcWebApiProvider, IMachineKeyProvider machineKeyProvider, ISystemInfoProvider systemInfoProvider, ICultureInfoProvider cultureInfoProvider) : base(icmcWebApiProvider, machineKeyProvider, systemInfoProvider, cultureInfoProvider)
        {

        }


        protected override EventParams GetApplicationLaunchedParams()
        {
            var eventParams = new EventParams()
            {
                EventParamValue = "eh_pu",
                EventId = "ei_da",
                EventGroupId = "eg_da"
            };
            return eventParams;
        }


        //5)       Valkyrie Unknown File Upload
        public void ValkyrieUnkownFileUpload(IValkyrieUnkownFileUploadWithTrigger request)
        {

            var additionalParameters = new List<IParameter>
            {
                new Parameter
                {
                    IsApplication = false,
                    Name = "vu_tfc",
                    Value = request.TotalFilesCount.ToString()
                },
                new Parameter
                {
                    IsApplication = false,
                    Name = "vu_mfc",
                    Value = request.MalwareFilesCount.ToString()
                },
                new Parameter
                {
                    IsApplication = false,
                    Name = "vu_trg",
                    Value = ((int)request.UploadTrigger).ToString()

                }
            };

            EventParams eventParams = new EventParams
            {
                AdditionalParameters = additionalParameters,
                EventGroupId = "eg_da",
                EventParamValue = "eh_ufh_vu",
                EventId = "ei_rp_vu"
            };
            SendData(request, eventParams);
        }
        //6)       Valkyrie Unknown File Upload Updated
        public void ValkyrieUnkownFileUploadUpdated(IValkyrieUnkownFileUpload request)
        {
            List<IParameter> additionalParameters = new List<IParameter>
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


            EventParams eventParams = new EventParams
            {
                AdditionalParameters = additionalParameters,
                EventGroupId = "eg_da",
                EventParamValue = "eh_ufh_vuu",
                EventId = "ei_rp_vuu"
            };

            SendData(request, eventParams);
        }

    }
}
