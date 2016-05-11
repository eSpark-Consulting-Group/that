using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using APTAT.Core.Descent;

namespace APTAT.Core.Reports
{
    public interface IAptReportBuilder
    {
        void BeginBuildReport(Guid operationUid, ReportType reportType, object state = null);
        void BeginBuildReport(AptScanOperationContext operationContext, ReportType reportType, object state = null);
        event EventHandler<ReportCompletedEventArgs> ReportCompleted;
    }
}
