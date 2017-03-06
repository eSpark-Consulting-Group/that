using APTAT.Core.Reports;

namespace APTAT.Core.TelerikReports
{
    public interface IReportFileGenerator
    {
        byte[] Build(AptReportData data, ReportType reportType, ReportFormat reportFormat);

    }
}