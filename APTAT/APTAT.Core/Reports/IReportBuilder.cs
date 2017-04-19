using System.Threading.Tasks;

namespace APTAT.Core.Reports
{
    public interface IReportBuilder
    {
        Task<ReportFile> BuildReportAsync(ReportData reportData, ReportType reportType, bool saveFile = true);        
    }
}