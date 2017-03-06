using APTAT.Core.TelerikReports;
using System;
using System.Linq;

namespace APTAT.Core.Reports
{
    public class ReportBuilderFAT : ReportBuilder
    {
        protected override byte[] Render(AptReportData data, ReportType reportType)
        {
            if (data == null) return null;
            var renderer = RenderStrategyFactory.GetRenderStrategy(ReportFormat.PDF);

            switch (reportType)
            {
                case ReportType.ExecutiveWithValkyrie:
                    data.GetAllBadFilesSorted(false);
                    return renderer.Render(new ExecutiveReportFAT(), data);

                case ReportType.DetailedPerDeviceWithValkyrie:
                    var valkyrieDetailedDevicesReport = new DetailedDevicesReportFAT();
                    var topVulnerableDevicesValk = data.Top10VulnerableDevicesValkyrie.Select(d => d.BadFilesCount).DefaultIfEmpty().Max();
                    if (topVulnerableDevicesValk <= 5)
                    {
                        valkyrieDetailedDevicesReport.ChangeScaleForOne();
                    }
                    return renderer.Render(valkyrieDetailedDevicesReport, data);

                case ReportType.DetailedPerProgramWithValkyrie:
                    data.SucceededTargetsWithValkyrie.ToArray();
                    return renderer.Render(new DetailedDevicesReportFAT(), data);

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType));
            }

        }

    }
}
