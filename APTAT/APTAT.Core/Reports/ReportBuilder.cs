using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using APTAT.Core.TelerikReports;

namespace APTAT.Core.Reports
{
    public class ReportBuilder
    {
        private string _fileName;

        public delegate string BuildReportDelegate(AptReportData data, ReportType reportType);

        private readonly ReportNamesCache _cache = ReportNamesCache.Instance;
        public string Build(AptReportData data, ReportType reportType)
        {
            if (data == null)
                return null;

            var existentName = string.Empty;
            var flsReportKey = data.OperationUid + "-" + data.FlsLookupOperationUid + "-" + reportType;
            var valkyrieReportKey = data.ValkyrieOperationUid + "-" + data.FlsLookupOperationUid + "-" + reportType;
            var reportKey = string.Empty;
            if (_cache.Reports.Keys.Contains(flsReportKey))
            {
                existentName = _cache.Reports[flsReportKey];
                reportKey = flsReportKey;
            }
            else if (_cache.Reports.Keys.Contains(valkyrieReportKey))
            {
                existentName = _cache.Reports[valkyrieReportKey];
                reportKey = valkyrieReportKey;
            }
            if (!string.IsNullOrWhiteSpace(existentName))
            {
                if (File.Exists(existentName))
                {
                    _fileName = existentName;
                    return existentName;
                }
                _cache.Reports.Remove(reportKey);
            }
            
            _fileName = GetFileName(reportType);

            switch (reportType)
            {
                case ReportType.Executive:
                    _cache.Reports.Add(flsReportKey, _fileName);
                    _reportFileGenerator.Build(_fileName, data, TelerikReports.ReportType.ExecutiveReport, ReportFormat.PDF, flsReportKey);
                    break;

                case ReportType.DetailedPerDevice:
                    _cache.Reports.Add(flsReportKey, _fileName);
                    _reportFileGenerator.Build(_fileName, data, TelerikReports.ReportType.DetailedPerDeviceReport, ReportFormat.PDF, flsReportKey);
                    break;

                case ReportType.DetailedPerProgram:
                    _cache.Reports.Add(flsReportKey, _fileName);
                    _reportFileGenerator.Build(_fileName, data, TelerikReports.ReportType.DetailedPerProgramReport, ReportFormat.PDF, flsReportKey);
                    break;

                case ReportType.ExecutiveWithValkyrie:
                    _cache.Reports.Add(valkyrieReportKey, _fileName);
                    BuildExecutiveWithValkyrieReport(_fileName, data, valkyrieReportKey);
                    break;

                case ReportType.DetailedPerDeviceWithValkyrie:
                    _cache.Reports.Add(valkyrieReportKey, _fileName);
                    BuildDetailedPerDeviceWithValkyrieReport(_fileName, data, valkyrieReportKey);
                    break;

                case ReportType.DetailedPerProgramWithValkyrie:
                    _cache.Reports.Add(valkyrieReportKey, _fileName);
                    _reportFileGenerator.Build(_fileName, data, TelerikReports.ReportType.DetailedPerProgramValkyrieReport, ReportFormat.PDF, valkyrieReportKey);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType));
            }

            return _fileName;
        }

        private readonly IReportFileGenerator _reportFileGenerator = new ReportFileGenerator(new FileHelper());

        private void BuildExecutiveWithValkyrieReport(string fileName, AptReportData data, string uid)
        {
            data.GetAllBadFilesSorted(false);
            _reportFileGenerator.Build(fileName, data, TelerikReports.ReportType.ExecutiveValkyrieReport, ReportFormat.PDF, uid);
        }

        private void BuildDetailedPerDeviceWithValkyrieReport(string fileName, AptReportData data, string uid)
        {
            var x = data.SucceededTargetsWithValkyrie.ToArray();
            _reportFileGenerator.Build(fileName, data, TelerikReports.ReportType.DetailedPerDeviceWithValkyrieReport, ReportFormat.PDF, uid);
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