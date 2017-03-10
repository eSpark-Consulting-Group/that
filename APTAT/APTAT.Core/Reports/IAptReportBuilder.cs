using System;

namespace APTAT.Core.Reports
{
    internal interface TestOne
    {
        void DoIt();
    }
    internal interface IAptReportBuilderInternal : IAptReportBuilder
    {
        void Init(IAptAtManagerInternal aptAtManager);
    }
    public interface IAptReportBuilder
    {
        void BeginLastBuildReport(ReportType reportType, bool saveFile = true, object state = null);
        
        event EventHandler<ReportCompletedEventArgs> ReportCompleted;
    }
}
