using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Valkyrie;

namespace APTAT.Core.Valkyrie
{
    class ValkyrieTrackableInfo : IValkyrieTrackableInfo
    {
        public ValkyrieTrackableInfo(DateTime? lastModifiedDate, IValkyrieAnalysisResult[] results)
        {
            LastModifiedDate = lastModifiedDate;
            Results = results;
        }

        public DateTime? LastModifiedDate { get;  }
        public IValkyrieAnalysisResult[] Results { get; }
    }

    public interface IValkyrieTrackableInfo
    {
        DateTime? LastModifiedDate { get;  }
        IValkyrieAnalysisResult[] Results { get; }
    }
}
