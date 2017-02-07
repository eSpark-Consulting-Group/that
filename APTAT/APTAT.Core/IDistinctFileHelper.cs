using System;
using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;
using System.Collections.Generic;

namespace APTAT.Core
{
    public interface IDistinctFileHelper
    {
        List<ValkyrieFileInfo> GetDistinctFileInfo(IAptScanResult[] results);
    }
}