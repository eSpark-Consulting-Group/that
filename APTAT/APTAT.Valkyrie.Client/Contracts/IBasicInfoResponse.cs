using System;
using System.Collections.Generic;
using Valkyrie.WebApiProvider.Enums;

namespace Valkyrie.WebApiProvider.Contracts
{
    public interface IBasicInfoResponse
    {
        FileCategories[] CategoryIds { get;  }
        Dictionary<string, string> FilesystemPermissions { get;  }
        DateTime? FirstAnalysisDate { get;  }
        bool IsWhiteListed { get;  }
        DateTime? LastAnalysisDate { get;  }
        float? MalwareProbability { get;  }
        bool Presence { get;  }
        string ResultDescription { get;  }
        string ResultMessage { get;  }
        ErrorCodesEnum ReturnCode { get;  }
        string SHA1 { get;  }
        string SubmitToken { get;  }
        bool Upload { get;  }
        ValkyrieVerdict Verdict { get;  }
        string VerdictSource { get;  }
    }
}