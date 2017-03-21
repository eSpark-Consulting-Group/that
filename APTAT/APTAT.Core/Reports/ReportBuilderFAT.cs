using APTAT.Core.TelerikReports;
using System;
using System.Linq;
using APTAT.Core.Apt;
using System.Collections.Generic;

namespace APTAT.Core.Reports
{
    public class ReportBuilderFAT : ReportBuilder
    {

        public override AptReportData CreateReportData(IResultMergeHelper mergeHelper, IAggregatedAptScanResult operationContext, Dictionary<string, FileResult> valkyrieAnalysisResult, Guid ValkyrieOperationUid, Guid FlsLookupOperationUid)
        {
            var data = new AptReportDataFAT(mergeHelper)
            {
                OperationUid = operationContext.OperationUid,
                ScanStarted = operationContext.StartedTime.ToLocalTime(),
                ScanFinished = (DateTime)operationContext.CompletedTime?.ToLocalTime(),
                TargetsFailed = operationContext.FailedTargetsCount,
                TargetsNotPerformed = operationContext.NotPerformedTargetsCount,
                TotalTargetsCount = operationContext.TotalTargetsCount,
                TargetsSucceeded = operationContext.SucceededTargetsCount,
                ValkyrieAnalysisResult = valkyrieAnalysisResult
                       .ToDictionary(k => k.Key, k => k.Value, StringComparer.OrdinalIgnoreCase),
                ValkyrieOperationUid = ValkyrieOperationUid,
                FlsLookupOperationUid = FlsLookupOperationUid,
                DevicesSourceDescription = operationContext.DeviceSourceDescription
            };
            return data;
        }

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
                    return renderer.Render(new DetailedProgramsReportFAT(), data);

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType));
            }

        }

    }
}
