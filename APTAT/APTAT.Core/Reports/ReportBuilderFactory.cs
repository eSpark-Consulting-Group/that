using System;

namespace APTAT.Core.Reports
{
    public class ReportBuilderFactory : IReportBuilderFactory
    {
        protected IReportNamesCache Cache { get; }
        protected IResultMergeHelper MergeHelper { get; }

        public ReportBuilderFactory(IReportNamesCache cache, IResultMergeHelper mergeHelper)
        {
            Cache = cache;
            MergeHelper = mergeHelper;
        }

        public virtual  IReportBuilder GetReportBuilder(ReportScanStage stage)
        {
            switch (stage)
            {
                case ReportScanStage.FlsScan:
                    return new ReportBuilder(Cache, MergeHelper);
                    
                case ReportScanStage.ValkyrieScan:
                    return  new ReportBuilderValkyrie(Cache, MergeHelper);
                default:
                    throw new NotSupportedException(nameof(stage));
            }
        }
    }
}