using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Core.Valkyrie
{
    public class ValkyrieAnalyseOperationContext
    {
        public ValkyrieFileInfo ValkyrieFileInfo { get; set; }
        
        public string AnalysisId { get; set; }
        public string HashSha1 { get; set; }
    }
}
