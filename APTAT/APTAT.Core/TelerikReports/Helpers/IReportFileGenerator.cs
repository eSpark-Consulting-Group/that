using APTAT.Core.Reports;

namespace APTAT.Core.TelerikReports
{
    public interface IReportFileGenerator
    {
        string Build(string fileName, AptReportData data, ReportType reportType, ReportFormat reportFormat, string operationUid);

    }
}