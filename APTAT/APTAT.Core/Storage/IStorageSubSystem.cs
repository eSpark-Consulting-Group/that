using System;
using System.Collections.Generic;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Valkyrie;
using APTAT.DAL.Enums;

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
        void SaveValkyrieResults(int scanId, IValkyrieAnalysisResult[] valkyrieResults);
        void TryFinishValkyrieScan(int scanId, bool isCancelled);
        void TryFinishComputersValkyrieScan(int scanId);
        void FailValkyrieScan(int scanId, bool isCancelled);
        void SaveOrUpdateValkyrieResult(int scanId, IValkyrieAnalysisResult[] valkyrieResults);
        void UpdateEmails(string[] emails);
    }
}
