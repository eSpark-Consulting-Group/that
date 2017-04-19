namespace APTAT.Core.Reports
{
    public class ReportBuilderValkyrie : ReportBuilder
    {
        public ReportBuilderValkyrie(IReportNamesCache cache) : base(cache)
        {
        }

        protected override AptReportData CreateReportData(ReportData reportData)
        {
            return new AptReportDataValkyrie(reportData, new FinalVerdictCalculator());
        }
    }
}
