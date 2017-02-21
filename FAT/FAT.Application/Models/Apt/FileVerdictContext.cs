using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Core.Descent.Apt;
using FlsResult = APTAT.Core.Descent.Apt.CloudVerdict;
using APTAT.Valkyrie;

namespace APTAT.Application.Models.Apt
{
    public class FileVerdictContext : IFileVerdictContext
    {
        public string Sha1 { get;  }
        public FlsResult? FlsVerdict { get;  }
        public DateTime? FlsVerdictModifiedDate { get;  }
        public ValkyrieAnalysisResult ValkyrieAnalysisResult { get;  }
        public DateTime? ValkyrieModifiedDate { get; set; }
        public FileVerdictContext(string sha1, FlsResult flsVerdict, DateTime modifiedDate)
        {
            Sha1 = sha1;
            FlsVerdictModifiedDate = modifiedDate;
            FlsVerdict = flsVerdict;
        }

        public FileVerdictContext(string sha1, ValkyrieAnalysisResult valkyrieAnalysisResult, DateTime modifiedDate)
        {
            Sha1 = sha1;
            ValkyrieModifiedDate = modifiedDate;
            ValkyrieAnalysisResult = valkyrieAnalysisResult;
        }
    }
}
