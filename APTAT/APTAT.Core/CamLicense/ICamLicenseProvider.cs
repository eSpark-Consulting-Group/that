using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Core.CamLicense
{
    public interface ICamLicenseProvider
    {
        string TryGetFreeLicense();
    }
}
