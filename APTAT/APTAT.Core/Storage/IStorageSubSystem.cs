using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.DAL.Entities;
using APTAT.DAL.Enums;
using APTAT.Valkyrie;

namespace APTAT.Core.Storage
{
    public interface IStorageSubSystem
    {
        void SaveOperationContext(IAptScanOperationContext aptScanner);
        void UpdateScanResults(IAptScanOperationContext scanOperationContext);
        void SaveScanResults(Guid operationUid, IAptScanResult scanResult);
        void UpdateVerdictForOperation(Guid operationUid, Dictionary<string, Descent.Apt.CloudVerdict> resultsWithoutErrors,
            VerdictSource verdictSource);

    }
    public interface IValkyrieStorageSubSystem
    {
        int SaveValkyrieScan(Guid operationUid, string apiKey, string scanId);
        void SaveValkyrieResults(int scanId, ValkyrieAnalysisResult[] valkyrieResults);
        void TryFinishValkyrieScan(int scanId, bool isCancelled);
        void TryFinishComputersValkyrieScan(int scanId);
        void FailValkyrieScan(int scanId, bool isCancelled);
        void UpdateValkyrieResult(int scanId, ValkyrieAnalysisResult[] valkyrieResults);
        void UpdateEmails(string[] emails);
    }
}
