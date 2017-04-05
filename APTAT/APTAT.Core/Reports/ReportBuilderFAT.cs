using APTAT.Core.TelerikReports;
using System;
using System.Globalization;

namespace APTAT.Core.Reports
{
    public class ReportBuilderFAT : ReportBuilder
    {
        public ReportBuilderFAT(IReportNamesCache cache, IResultMergeHelper mergeHelper) : base(cache, mergeHelper)
        {
        }

        protected override AptReportData CreateReportData(ReportData reportData)
        {
            return new AptReportDataFAT(MergeHelper, reportData);
        }

        protected override string GetFileName(ReportType reportType)
        {
            var timeStamp = DateTime.Now.ToString("s", CultureInfo.CurrentCulture).Replace(":", "").Replace("-", "");
            const string fileFormatString = "{0}-{1}.pdf";
            switch (reportType)
            {
                case ReportType.DetailedPerDeviceWithValkyrie:
                    return string.Format(fileFormatString, "PerDeviceReport", timeStamp);
                case ReportType.ExecutiveWithValkyrie:
                    return string.Format(fileFormatString, "ExecutiveReport", timeStamp);
                case ReportType.DetailedPerProgramWithValkyrie:
                    return string.Format(fileFormatString, "PerProgramReport", timeStamp);
                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType));
            }
        }

        protected override byte[] Render(AptReportData data, ReportType reportType)
        {
            if (data == null) return null;
            var renderer = RenderStrategyFactory.GetRenderStrategy(ReportFormat.PDF);

            switch (reportType)
            {
                case ReportType.ExecutiveWithValkyrie:
                    return renderer.Render(new ExecutiveReportFAT(), data);

                case ReportType.DetailedPerDeviceWithValkyrie:
                    return renderer.Render(new DetailedDeviceReportFAT(), data);

                case ReportType.DetailedPerProgramWithValkyrie:
                    return renderer.Render(new DetailedProgramsReportFAT(), data);

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType));
            }
        }
    }
}
