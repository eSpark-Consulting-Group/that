using System;

namespace APTAT.Core.Reports
{
    public class ReportBuilderFactory : IReportBuilderFactory
    {
        protected IReportNamesCache Cache { get; }
        private ResultMergeHelper MergeHelper { get; }

        public ReportBuilderFactory(IReportNamesCache cache )
        {
            Cache = cache;
            MergeHelper = new ResultMergeHelper();
        }

        public virtual  IReportBuilder GetReportBuilder(ReportScanStage stage)
        {
            switch (stage)
            {
                case ReportScanStage.FlsScan:
                    return new ReportBuilder(Cache);
                    
                case ReportScanStage.ValkyrieScan:
                    return  new ReportBuilderValkyrie(Cache);
                default:
                    throw new NotSupportedException(nameof(stage));
            }
        }
    }
}