using System;

namespace APTAT.Core.Reports
{
    public interface IAptReportBuilder
    {
        void BeginLastBuildReport(ReportType reportType, object state = null);
        event EventHandler<ReportCompletedEventArgs> ReportCompleted;
    }
}
