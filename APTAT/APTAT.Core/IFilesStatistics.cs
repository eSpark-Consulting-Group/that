namespace APTAT.Core
{
    public interface IFilesStatistics
    {
        int EmptyFilesCount { get;  }
        int FailedToAnalyzeFilesCount { get;  }
        int MalwareFilesCount { get;  }
        int NoVerdictFilesCount { get;  }
        int PendingAnalysisFilesCount { get;  }
        int SafeFilesCount { get;  }
        int SkippedFilesCount { get;  }
        int TotalFilesCount { get;  }
        int UnknownFilesCount { get;  }
    }
}