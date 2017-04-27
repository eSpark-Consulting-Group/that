using System;
using APTAT.Core.Descent.Apt;
using System.Collections.Generic;
using APTAT.Core.Valkyrie;

namespace APTAT.Core
{
    public interface IDistinctFileHelper
    {
        List<IValkyrieFileInfo> GetDistinctFileInfo(IAptScanResult[] results);
        Dictionary<string, IValkyrieFileInfo> GetDistinctFileInfoDict(IAptScanResult[] results);
        bool ShouldSendToValkyrie(IFlsResult module);
    }
}