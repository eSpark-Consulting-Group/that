namespace APTAT.Core.CMC.Request
{
    public interface IReportPrintedOrSavedRequest : IRequest, IReportGeneratedRequest
    {
        ReportAction ReportAction { get; set; }
    }
}