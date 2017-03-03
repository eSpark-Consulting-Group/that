using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.DAL.Entities;

namespace APTAT.Core.Storage
{
    public interface IStorageSubSystem
    {
        void SaveOperationContext(IAptScanOperationContext aptScanner);
        void UpdateScanResults(IAptScanOperationContext scanOperationContext);
        void SaveScanResults(Guid operationUid, IAptScanResult scanResult);
        void UpdateVerdictForOperation(Guid operationUid, Dictionary<string, Descent.Apt.CloudVerdict> resultsWithoutErrors,
            VerdictSource verdictSource);
        int SaveValkyrieScan(Guid operationUid, string apiKey, string scanId);
        void SaveValkyrieResults(int scanId, Dictionary<string, FileResult>.ValueCollection valkyrieResults);
        void UpdateValkyrieResult(int scanId, Dictionary<string, FileResult>.ValueCollection valkyrieResults);

    }
}
