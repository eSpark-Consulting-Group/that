using System;

namespace APTAT.Core.Reports
{
    public class ReportBuilderFactoryFat : ReportBuilderFactory
    {
        public ReportBuilderFactoryFat(IReportNamesCache cache, IResultMergeHelper mergeHelper) : base(cache, mergeHelper)
        {
        }

        public override IReportBuilder GetReportBuilder(ReportScanStage stage)
        {
            switch (stage)
            {
                case ReportScanStage.ValkyrieScan:
                    return new ReportBuilderFAT(Cache, MergeHelper);
                default:
                    throw new NotSupportedException(nameof(stage));
            }
        }
    }
}