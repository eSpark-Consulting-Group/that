using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.Core;
using FAT.Application.Properties;

namespace FAT.Application.Providers
{
    class CamLicenseProviderFAT : CamLicenseProvider
    {
        public CamLicenseProviderFAT(IMachineKeyProvider machineKeyProvider) : base(machineKeyProvider, Settings.Default.UseDevCam)
        {
            _pricingTermIdentifier = "misc.forensic.analysis.tool.free";
        }
    }
}
