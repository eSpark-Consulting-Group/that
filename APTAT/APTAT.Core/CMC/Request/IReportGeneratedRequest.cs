namespace APTAT.Core.CMC.Request
{
    public interface IReportGeneratedRequest: IRequest
    {
        ReportKind ReportKind { get; set; }
    }
}