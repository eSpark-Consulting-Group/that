namespace APTAT.Core.Reports
{
    public class ReportBuilderValkyrie : ReportBuilder
    {
        public ReportBuilderValkyrie(IReportNamesCache cache, IResultMergeHelper mergeHelper) : base(cache, mergeHelper)
        {
        }

        protected override AptReportData CreateReportData(ReportData reportData)
        {
            return new AptReportDataValkyrie(MergeHelper, reportData);
        }
    }
}
