namespace APTAT.Core.TelerikReports
{
    public interface IRenderStrategy
    {
        string Render(Telerik.Reporting.Report report, object data, string fileName);
    }
}
