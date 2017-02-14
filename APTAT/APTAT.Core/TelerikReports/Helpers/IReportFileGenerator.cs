namespace APTAT.Core.TelerikReports
{
    public interface IReportFileGenerator
    {
        string Build(string fileName, object data, ReportType reportType, ReportFormat reportFormat, string operationUid);
    }
}