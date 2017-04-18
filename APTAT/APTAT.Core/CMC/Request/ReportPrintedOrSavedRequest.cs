namespace APTAT.Core.CMC.Request
{
    public class ReportPrintedOrSavedRequest: IReportPrintedOrSavedRequest
    {
        public ReportKind ReportKind { get; set; }
        public ReportAction ReportAction { get; set; }
    }
}
