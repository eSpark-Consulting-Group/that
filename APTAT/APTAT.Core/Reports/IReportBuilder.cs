namespace APTAT.Core.Reports
{
    public interface IReportBuilder
    {
        ReportFile Build(AptReportData data, ReportType reportType, bool saveFile = true);
    }
}