using System;
using System.IO;

namespace APTAT.Core.Descent.Apt
{
    public interface IModule : IFlsResult
    {
        string Id { get;  }
        string Path { get;  }
        DateTime CloudDate { get;  }
        long Size { get;  }
        string Attributes { get;  }
        DateTime CreatedInternal { get;  }
        DateTime ModifiedInternal { get;  }
        DateTime AccessedInternal { get;  }
        DateTime Created { get; }
        DateTime Modified { get; }
        DateTime Accessed { get; }
        string Signer { get; set; }
        string Description { get;  }
        string Version { get;  }
        string Copyright { get;  }
        string Product { get;  }
        FileAttributes FileAttributes { get; }
        string FileSizeFormatted { get; }
        string FileAge { get; }
        bool IsBad { get; }
        bool IsUnknownOrFailed { get; }
    }
}