using System;
using FlsResult = APTAT.Core.Descent.Apt.CloudVerdict;
using APTAT.Core.Valkyrie;

namespace APTAT.Application.Models.Apt
{
    public class FileVerdictContext : IFileVerdictContext
    {
        public string Sha1 { get;  }
        public FlsResult? FlsVerdict { get;  }
        public DateTime? FlsVerdictModifiedDate { get;  }
        public IValkyrieAnalysisResult ValkyrieAnalysisResult { get;  }
        public DateTime? ValkyrieModifiedDate { get; set; }
        public FileVerdictContext(string sha1, FlsResult flsVerdict, DateTime modifiedDate)
        {
            Sha1 = sha1;
            FlsVerdictModifiedDate = modifiedDate;
            FlsVerdict = flsVerdict;
        }

        public FileVerdictContext(string sha1, IValkyrieAnalysisResult valkyrieAnalysisResult, DateTime modifiedDate)
        {
            Sha1 = sha1;
            ValkyrieModifiedDate = modifiedDate;
            ValkyrieAnalysisResult = valkyrieAnalysisResult;
        }
    }
}
