namespace APTAT.Core.TelerikReports
{
    public interface IRenderStrategy
    {    
        byte[] Render(Telerik.Reporting.Report report, object data);
    }
}
