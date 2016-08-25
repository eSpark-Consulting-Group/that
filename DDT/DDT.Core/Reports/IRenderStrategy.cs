namespace DDT.Core.Reports
{
    public interface IRenderStrategy
    {
        string Render(Telerik.Reporting.Report report, object data, string fileName);
    }
}
