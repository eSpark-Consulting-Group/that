namespace APTAT.Core.Reports
{
    public interface IReportNamesCache
    {
        ReportFile TryGetReportFromCache(string reportKey);
        void Add(string reportKey, string fileName);
    }
}