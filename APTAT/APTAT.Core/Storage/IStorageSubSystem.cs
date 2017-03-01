using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core.Storage
{
    public interface IStorageSubSystem
    {
        void SaveOperationContext(IAptScanOperationContext aptScanner);
        void UpdateScanResults(IAptScanOperationContext scanOperationContext);
        void SaveScanResults(Guid operationUid, IAptScanResult scanResult);
    }
}
