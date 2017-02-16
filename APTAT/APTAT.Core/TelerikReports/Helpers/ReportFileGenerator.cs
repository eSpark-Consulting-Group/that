using APTAT.Core.TelerikReports;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using APTAT.Core.Reports;

namespace APTAT.Core.TelerikReports
{

    public class ReportFileGenerator : IReportFileGenerator
    {
        private const string ExecutiveReportName = "ExecutiveReport";
        //private readonly IReportNamesCache _cache;
        private const string ReportDefinitionPath = "APTAT.Core.TelerikReports";
        //private readonly IReportRenderer _reportRenderer;
        private readonly IFileHelper _fileHelper;

        public ReportFileGenerator(IFileHelper fileHelper)
        {
            _fileHelper = fileHelper;            
        }

        //private string LookupForExistentReport(string operationUid, ReportType reportType, ReportFormat reportFormat)
        //{
        //    string existentName = _cache.Find(operationUid, reportType, reportFormat);

        //    if (!String.IsNullOrWhiteSpace(existentName))
        //    {
        //        if (_fileHelper.IsFileExists(existentName))
        //        {
        //            return existentName;
        //        }
        //        else
        //        {
        //            _cache.Remove(operationUid, reportType, reportFormat);
        //        }
        //    }
        //    return null;
        //}

        public string Build(string resultfileName, AptReportData data, ReportType reportType, ReportFormat reportFormat, string operationUid)
        {
            if (data == null) return null;
            //var foundReportFileName = LookupForExistentReport(operationUid, reportType, reportFormat);
            //if (!string.IsNullOrEmpty(foundReportFileName)) return foundReportFileName;
            //var fileName = GetFileName(reportType);
            var renderer = RenderStrategyFactory.GetRenderStrategy(reportFormat);
            var fName = Path.GetFileNameWithoutExtension(resultfileName);
            switch (reportType)
            {
                case ReportType.ExecutiveReport:
                    return renderer.Render(new ExecutiveReport(), data, fName);
                    //_cache.Add(operationUid, reportType, reportFormat, filePath);
                    
                case ReportType.DetailedPerProgramReport:
                     return renderer.Render(new DetailedPrograms(), data, fName);

                case ReportType.DetailedPerDeviceReport:
                    var ddrep = new DetailedDevicesReport();
                    var max = data.Top10VulnerableDevices.Select(d => d.BadFilesCount).Max();
                    if (max <= 5)
                    {
                        ddrep.ChangeScaleForOne();
                    }
                    
                    return renderer.Render(ddrep, data, fName);

                case ReportType.ExecutiveValkyrieReport:
                    return renderer.Render(new ExecutiveValkyrieReport(), data, fName);

                case ReportType.DetailedPerDeviceWithValkyrieReport:
                    return renderer.Render(new DetailedDevicesValkyrieReport(), data, fName);
                
               case ReportType.DetailedPerProgramValkyrieReport:
                    return renderer.Render(new DetailedProgramsValkyrieReport(), data, fName);

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType));
            }
        }

        private static string GetFileName(ReportType reportType)
        {
            var timeStamp = DateTime.Now.ToString("s", CultureInfo.CurrentCulture).Replace(":", "").Replace("-", "");

            const string FileFormatString = "{0}-{1}.pdf";
            switch (reportType)
            {
                case ReportType.ExecutiveReport:
                    return String.Format(FileFormatString, ExecutiveReportName, timeStamp);

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType));
            }
        }

    }
}
