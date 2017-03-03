using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.DAL.Enums
{
    public enum CloudVerdict
    {
        Safe, // safe
        Unknown, // unknown
        Malware, // malware
        FirstSeen, // doesntexist
        FailedToAnalyze, // error
        NoVerdict // servererror
    }
}
