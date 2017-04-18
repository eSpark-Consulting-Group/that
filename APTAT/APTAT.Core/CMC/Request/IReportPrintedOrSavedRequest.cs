namespace APTAT.Core.CMC.Request
{
    public interface IReportPrintedOrSavedRequest : IRequest
    {
        ReportKind ReportKind { get; set; }
        ReportAction ReportAction { get; set; }
    }
}