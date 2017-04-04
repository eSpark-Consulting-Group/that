using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using APTAT.Core.TelerikReports;
using APTAT.Core.Properties;
using APTAT.Core.Apt;
using APTAT.Valkyrie;

namespace APTAT.Core.Reports
{
    public class ReportBuilder : IReportBuilder
    {
        public ReportBuilder()
        {

        }
        protected virtual byte[] Render(AptReportData data, ReportType reportType)
        {
            if (data == null) return null;

            var renderer = RenderStrategyFactory.GetRenderStrategy(ReportFormat.PDF);

            switch (reportType)
            {
                case ReportType.Executive:
                    return renderer.Render(new ExecutiveReport(), data);

                case ReportType.DetailedPerProgram:
                    return renderer.Render(new DetailedPrograms(), data);

                case ReportType.DetailedPerDevice:
                    var ddrep = new DetailedDevicesReport();
                    var max = data.Top10VulnerableDevices.Select(d => d.BadFilesCount).DefaultIfEmpty().Max();
                    if (max <= 5)
                    {
                        ddrep.ChangeScaleForOne();
                    }

                    return renderer.Render(ddrep, data);

                case ReportType.ExecutiveWithValkyrie:
                    data.GetAllBadFilesSorted(false);
                    return renderer.Render(new ExecutiveValkyrieReport(), data);

                case ReportType.DetailedPerDeviceWithValkyrie:
                    var valkyrieDetailedDevicesReport = new DetailedDevicesValkyrieReport();
                    var topVulnerableDevicesValk = data.Top10VulnerableDevicesValkyrie.Select(d => d.BadFilesCount).DefaultIfEmpty().Max();
                    if (topVulnerableDevicesValk <= 5)
                    {
                        valkyrieDetailedDevicesReport.ChangeScaleForOne();
                    }
                    return renderer.Render(valkyrieDetailedDevicesReport, data);

                case ReportType.DetailedPerProgramWithValkyrie:
                    data.SucceededTargetsWithValkyrie.ToArray();
                    return renderer.Render(new DetailedProgramsValkyrieReport(), data);

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType));
            }
        }

        public delegate ReportFile BuildReportDelegate(AptReportData data, ReportType reportType, bool createFile);

        private readonly ReportNamesCache _cache = ReportNamesCache.Instance;

        private string GetReportCacheKey(AptReportData data, ReportType reportType)
        {
            switch (reportType.ScanStage())
            {
                case ReportScanStage.FlsScan:
                    return data.OperationUid + "-" + data.FlsLookupOperationUid + "-" + reportType;

                case ReportScanStage.ValkyrieScan:
                    return data.ValkyrieOperationUid + "-" + data.FlsLookupOperationUid + "-" + reportType;
                default:
                    throw new NotSupportedException("Scan Stage not supported " + reportType.ScanStage());

            }
        }

        private ReportFile TryGetReportFromCache(string reportKey)
        {
            var existentName = string.Empty;
            if (_cache.Reports.Keys.Contains(reportKey))
            {
                existentName = _cache.Reports[reportKey];
            }

            if (!string.IsNullOrWhiteSpace(existentName))
            {
                if (File.Exists(existentName))
                {
                    var data = File.ReadAllBytes(existentName);
                    return new ReportFile() { FileName = existentName, Data = data };
                }
                else
                {
                    _cache.Reports.Remove(reportKey);
                }
            }
            return null;
        }

        public ReportFile Build(AptReportData data, ReportType reportType, bool saveFile = true)
        {
            if (data == null) return null;

            var reportKey = GetReportCacheKey(data, reportType);
            var reportFromCache = TryGetReportFromCache(reportKey);
            if (reportFromCache != null) return reportFromCache;


            byte[] reportBytes = new byte[1];
            reportBytes = Render(data, reportType);

            string fileName = GetFileName(reportType);
            if (saveFile)
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    fs.Write(reportBytes, 0, reportBytes.Length);
                }
                _cache.Reports.Add(reportKey, fileName);
            }

            var path = Path.Combine(Settings.Default.ReportFolder, fileName);
            return new ReportFile() { FileName = path, Data = reportBytes };
        }


        private static string GetFileName(ReportType reportType)
        {
            var timeStamp = DateTime.Now.ToString("s", CultureInfo.CurrentCulture).Replace(":", "").Replace("-", "");
            const string fileFormatString = "{0}-{1}.pdf";
            switch (reportType)
            {
                case ReportType.Executive:
                    return string.Format(fileFormatString, "ExecutiveAptReport", timeStamp);
                case ReportType.ExecutiveWithValkyrie:
                    return string.Format(fileFormatString, "ExecutiveAptReportValkyrie", timeStamp);
                case ReportType.DetailedPerDevice:
                    return string.Format(fileFormatString, "DetailedPerDeviceAptReport", timeStamp);
                case ReportType.DetailedPerDeviceWithValkyrie:
                    return string.Format(fileFormatString, "DetailedPerDeviceAptReportValkyrie", timeStamp);
                case ReportType.DetailedPerProgram:
                    return string.Format(fileFormatString, "DetailedPerProgramAptReport", timeStamp);
                case ReportType.DetailedPerProgramWithValkyrie:
                    return string.Format(fileFormatString, "DetailedPerProgramAptReportValkyrie", timeStamp);

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType));
            }
        }

        public virtual AptReportData CreateReportData(IResultMergeHelper mergeHelper, IAggregatedAptScanResult operationContext,
            Dictionary<string, ValkyrieAnalysisResult> valkyrieAnalysisResult, Guid ValkyrieOperationUid, Guid FlsLookupOperationUid)
        {
            var data = new AptReportData(mergeHelper)
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
    }

    internal sealed class ReportNamesCache
    {
        private static readonly ReportNamesCache instance = new ReportNamesCache();
        static ReportNamesCache() { }
        private ReportNamesCache() { }
        public Dictionary<string, string> Reports = new Dictionary<string, string>();
        public static ReportNamesCache Instance => instance;
    }
}