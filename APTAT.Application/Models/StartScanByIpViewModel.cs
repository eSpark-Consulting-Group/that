using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Application.Models
{
    public class StartScanByIpViewModel : WizardViewModel
    {
        public const String SelectIp = "SelectIp";
        public const String SelectTargets = "SelectTargets";

        public StartScanByIpViewModel()
        {
            SetSteps(new[] { null, SelectIp, SelectTargets });

        }
    }
}
