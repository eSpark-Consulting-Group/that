using APTAT.Core.Valkyrie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Core.Valkyrie
{
    public interface IValkyrieClientEx : IValkyrieClient
    {
        void StartAnalyze(ICamRegistration camRegistration, string login, string password, int machineCount,
            NetworkCredential proxyCredential, string productName, Guid operaionUid);
        void TryFinishUsersScanOnStorage();
    }
}
