using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.Core;
using UFH.Application.Properties;

namespace UFH.Application.Providers
{
    class CamLicenseProviderUFH : CamLicenseProvider
    {
        public CamLicenseProviderUFH(IMachineKeyProvider machineKeyProvider) : base(machineKeyProvider, Settings.Default.UseDevCam)
        {
            _pricingTermIdentifier = "misc.unknown.file.hunter.free";//"misc.forensic.analysis.tool.free";//
        }
    }
}
