using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Application.Models
{
    public class StartScanWorkgroupViewModel : WizardViewModel
    {
        public const String SelectWorkgroup = "SelectWorkgroup";
        public const String SelectTargets = "SelectTargets";

        public StartScanWorkgroupViewModel()
        {
            SetSteps(new[] { null, SelectWorkgroup, SelectTargets });

        }
    }
}
