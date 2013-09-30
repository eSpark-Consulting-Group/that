using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Application.Models
{
    public class StartScanByIpViewModel : WizardViewModel
    {
        public StartScanByIpViewModel()
        {
            SetSteps(new[] { "" });

        }
    }
}
