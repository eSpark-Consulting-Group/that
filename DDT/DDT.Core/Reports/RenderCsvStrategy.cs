using DDT.Core.Helper;
using System.Linq;
using System.Text;

namespace DDT.Core.Reports
{
    public class RenderCsvStrategy : IRenderStrategy
    {
        public string Render(Telerik.Reporting.Report report, object data, string fileName)
        {
            var path = fileName + ".csv";
            const string separator = ",";
            ReportData dt = (ReportData) data;
            StringBuilder sb = new StringBuilder();
            if (dt.DeploymentByIPRange.Details.Any())
            {
                sb.AppendLine("Discovered By Network Addresses");
                var nwrk = CSVHelper.ToCsv(separator, dt.DeploymentByIPRange.Details);
                sb.Append(nwrk);
            }

            if (dt.DeploymentByWorkgroup.Details.Any())
            {
                sb.AppendLine("Discovered By Workgroup");
                var wg = CSVHelper.ToCsv(separator, dt.DeploymentByWorkgroup.Details);
                sb.Append(wg);
            }

            if (dt.DeploymentByDomain.Details.Any())
            {
                sb.AppendLine("Discovered By Domain");
                var dmn = CSVHelper.ToCsv(separator, dt.DeploymentByDomain.Details);
                sb.Append(dmn);
            }


            System.IO.File.WriteAllText(path, sb.ToString());
            return path;
        }
    }
}
