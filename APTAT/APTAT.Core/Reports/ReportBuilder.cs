using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using APTAT.Core.TelerikReports;
using APTAT.Core.Properties;

namespace APTAT.Core.Reports
{
    public class ReportBuilder
    {
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
            switch (reportType)
            {
                case ReportType.Executive:
                    reportBytes = _reportFileGenerator.Build(data, TelerikReports.ReportType.ExecutiveReport, ReportFormat.PDF);
                    break;

                case ReportType.DetailedPerDevice:
                    reportBytes = _reportFileGenerator.Build(data, TelerikReports.ReportType.DetailedPerDeviceReport, ReportFormat.PDF);
                    break;

                case ReportType.DetailedPerProgram:
                    reportBytes = _reportFileGenerator.Build(data, TelerikReports.ReportType.DetailedPerProgramReport, ReportFormat.PDF);
                    break;

                case ReportType.ExecutiveWithValkyrie:
                    reportBytes = BuildExecutiveWithValkyrieReport(data);
                    break;

                case ReportType.DetailedPerDeviceWithValkyrie:
                    reportBytes = BuildDetailedPerDeviceWithValkyrieReport(data);
                    break;

                case ReportType.DetailedPerProgramWithValkyrie:
                    reportBytes = _reportFileGenerator.Build(data, TelerikReports.ReportType.DetailedPerProgramValkyrieReport, ReportFormat.PDF);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType));
            }

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

        private readonly IReportFileGenerator _reportFileGenerator = new ReportFileGenerator(new FileHelper());

        private byte[] BuildExecutiveWithValkyrieReport(AptReportData data)
        {
            data.GetAllBadFilesSorted(false);
            return _reportFileGenerator.Build(data, TelerikReports.ReportType.ExecutiveValkyrieReport, ReportFormat.PDF);
        }

        private byte[] BuildDetailedPerDeviceWithValkyrieReport(AptReportData data)
        {
            data.SucceededTargetsWithValkyrie.ToArray();
            return _reportFileGenerator.Build(data, TelerikReports.ReportType.DetailedPerDeviceWithValkyrieReport, ReportFormat.PDF);
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