using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core
{
    public static class FlsResultExt
    {
        public static Boolean IsNotAnalyzed(this IFlsResult result)
        {
            return result.Cloud == CloudVerdict.FailedToAnalyze || result.Cloud == CloudVerdict.NoVerdict;
        }
    }
  public  interface IFlsResult
    {
     
        string Sha1 { get; set; }
        CloudVerdict Cloud { get; set; }
    }

    
    
}
