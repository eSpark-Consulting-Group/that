using System;
using System.Globalization;
using System.IO;
using System.Linq;
using APTAT.Core.TelerikReports;
using APTAT.Core.Properties;
using System.Threading.Tasks;
using System.Diagnostics;
using APTAT.Common;

namespace APTAT.Core.Reports
{
    public class ReportBuilder : IReportBuilder
    {
        private readonly IReportNamesCache _cache;
        

        public ReportBuilder(IReportNamesCache cache )
        {
            _cache = cache;
        }             
               
        public async Task<ReportFile> BuildReportAsync(ReportData reportData, ReportType reportType, bool saveFile = true)
        {
            ReportFile reportFile = new ReportFile();

            try
            {
                return await Task.Factory.StartNew(() =>
                {
                    var reportKey = reportData.OperationUid + "-" + reportType;
                    var reportFromCache = _cache.TryGetReportFromCache(reportKey);
                    if (reportFromCache != null) return reportFromCache;

                    var data = CreateReportData(reportData);
                    var reportBytes = Render(data, reportType);

                    string fileName = GetFileName(reportType);
                    var path = Path.Combine(Settings.Default.ReportFolder, fileName);

                    if (saveFile)
                    {
                        var directoryInfo = (new FileInfo(path)).Directory;
                        directoryInfo?.Create();


                        using (FileStream fs = new FileStream(path, FileMode.Create))
                        {
                            fs.Write(reportBytes, 0, reportBytes.Length);
                        }
                        _cache.Add(reportKey, fileName);
                    }

                    return new ReportFile { FileName = path, DocumentBytes = reportBytes };
                });
            }
            catch (Exception ex)
            {
                LogHelper.Trace(TraceEventType.Error, TraceCategory.Reports, "Report failed: {0}", ex);
                reportFile.Error = ex;
            }
            finally
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
            }
            return reportFile;
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
                    return renderer.Render(new ExecutiveValkyrieReport(), data);

                case ReportType.DetailedPerDeviceWithValkyrie:
                    var valkyrieDetailedDevicesReport = new DetailedDevicesValkyrieReport();
                    return renderer.Render(valkyrieDetailedDevicesReport, data);

                case ReportType.DetailedPerProgramWithValkyrie:
                    //data.SucceededTargets.ToArray();
                    return renderer.Render(new DetailedProgramsValkyrieReport(), data);

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType));
            }
        }
               
        protected virtual string GetFileName(ReportType reportType)
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

        protected virtual AptReportData CreateReportData(ReportData operationData)
        {
            return new AptReportData(operationData);
        }

    }
}