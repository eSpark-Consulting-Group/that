using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Core.Reports
{
    public class ReportBuilderFactoryUfh : ReportBuilderFactory
    {
        public ReportBuilderFactoryUfh(IReportNamesCache cache) : base(cache)
        {
        }

        public override IReportBuilder GetReportBuilder(ReportScanStage stage)
        {
            switch (stage)
            {
                case ReportScanStage.ValkyrieScan:
                    return new ReportBuilderUFH(Cache);
                default:
                    throw new NotSupportedException(nameof(stage));
            }
        }
    }
}
