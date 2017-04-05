namespace APTAT.Core.Reports
{
    public interface IReportBuilderFactory
    {
        IReportBuilder GetReportBuilder(ReportScanStage stage);
    }
}