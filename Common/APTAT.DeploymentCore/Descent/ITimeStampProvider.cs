using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.DeploymentCore.Descent
{
    public interface ITimeStampProvider
    {
        TimeSpan GetCurretTimestamp();
    }
}
