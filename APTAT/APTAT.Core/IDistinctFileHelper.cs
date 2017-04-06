using System;
using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;
using System.Collections.Generic;

namespace APTAT.Core
{
    public interface IDistinctFileHelper
    {
        List<IValkyrieFileInfo> GetDistinctFileInfo(IAptScanResult[] results);
        Dictionary<string, IValkyrieFileInfo> GetDistinctFileInfoDict(IAptScanResult[] results);
        bool ShouldSendToValkyrie(IFlsResult module);
    }
}