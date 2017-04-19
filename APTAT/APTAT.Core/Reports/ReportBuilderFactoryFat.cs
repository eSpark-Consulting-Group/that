using System;

namespace APTAT.Core.Reports
{
    public class ReportBuilderFactoryFat : ReportBuilderFactory
    {
        public ReportBuilderFactoryFat(IReportNamesCache cache) : base(cache)
        {
        }

        public override IReportBuilder GetReportBuilder(ReportScanStage stage)
        {
            switch (stage)
            {
                case ReportScanStage.ValkyrieScan:
                    return new ReportBuilderFAT(Cache);
                default:
                    throw new NotSupportedException(nameof(stage));
            }
        }
    }
}