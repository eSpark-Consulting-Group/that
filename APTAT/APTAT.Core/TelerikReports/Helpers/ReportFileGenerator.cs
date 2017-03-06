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
        
        private const string ReportDefinitionPath = "APTAT.Core.TelerikReports";
        
        private readonly IFileHelper _fileHelper;

        public ReportFileGenerator(IFileHelper fileHelper)
        {
            _fileHelper = fileHelper;            
        }

        

        public byte[] Build(AptReportData data, ReportType reportType, ReportFormat reportFormat)
        {
            if (data == null) return null;
            
            var renderer = RenderStrategyFactory.GetRenderStrategy(reportFormat);
            
            switch (reportType)
            {
                case ReportType.ExecutiveReport:
                    return renderer.Render(new ExecutiveReport(), data);                    
                    
                case ReportType.DetailedPerProgramReport:
                     return renderer.Render(new DetailedPrograms(), data);

                case ReportType.DetailedPerDeviceReport:
                    var ddrep = new DetailedDevicesReport();
                    var max = data.Top10VulnerableDevices.Select(d => d.BadFilesCount).DefaultIfEmpty().Max();
                    if (max <= 5)
                    {
                        ddrep.ChangeScaleForOne();
                    }
                    
                    return renderer.Render(ddrep, data);

                case ReportType.ExecutiveValkyrieReport:
                    return renderer.Render(new ExecutiveValkyrieReport(), data);

                case ReportType.DetailedPerDeviceWithValkyrieReport:
                    var valkyrieDetailedDevicesReport = new DetailedDevicesValkyrieReport();
                    var topVulnerableDevicesValk = data.Top10VulnerableDevicesValkyrie.Select(d => d.BadFilesCount).DefaultIfEmpty().Max();
                    if (topVulnerableDevicesValk <= 5)
                    {
                        valkyrieDetailedDevicesReport.ChangeScaleForOne();
                    }
                    return renderer.Render(valkyrieDetailedDevicesReport, data);
                
               case ReportType.DetailedPerProgramValkyrieReport:
                    return renderer.Render(new DetailedProgramsValkyrieReport(), data);

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType));
            }
        }        

    }
}
