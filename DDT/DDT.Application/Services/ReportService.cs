using APTAT.Common;
using APTAT.DeploymentCore.Descent;
using DDT.Application.Models;
using DDT.Core;
using DDT.Core.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.DeploymentCore;


namespace DDT.Application.Services
{
    public class ReportService: IReportService
    {
        private readonly IReportBuilder _reportBuilder;

        public ReportService(IReportBuilder reportBuilder)
        {
            _reportBuilder = reportBuilder;
        }

        public async Task<Tuple<string, string>>  BuildDiscoveryAndDeploymentReport(DeploymentItem[] targetsByIp, DeploymentItem[] targetsByDomain, DeploymentItem[] targetsByWorkgroup, Guid operationUid)
        {
            var deploymentByDomain = MapTo(targetsByDomain);
            var deploymentByIPRange = MapTo(targetsByIp);
            var deploymentByWorkgroup = MapTo(targetsByWorkgroup);
            ReportData data = new ReportData(deploymentByWorkgroup, deploymentByIPRange, deploymentByDomain);
            data.OperationUid = operationUid.ToString();
            string pdfReportPath = await Task.Run( () => _reportBuilder.Build(data, ReportType.DeploymentReport, ReportFormat.PDF));
            string csvReportPath = await Task.Run(() => _reportBuilder.Build(data, ReportType.DeploymentReport, ReportFormat.CSV));

            return Tuple.Create(pdfReportPath, csvReportPath);
        }
        
        private static DeploymentReportModel MapTo(IEnumerable<DeploymentItem> items)
        {
            var details = items.Select(i => new DetailsOfScan
            {
                OS = i.OS,
                PackageName = i.PackageName,
                DeviceName = i.Name,
                IP = i.IP,
                Status = !i.Failed ? i.Stopping? "Stopped": "Succeeded" : "Failed",
                Details = i.Value,
                DeploymentStatus = i.CalculateDeploymentStatus(),                
                SystemFamilyKind = i.SystemKind.GetValueOrDefault().ToSystemFamilyKind(),                
                TargetUid = i.TargetUid
                
            }).ToList();
            var model = new DeploymentReportModel(details);
            return model;
        }
    }
}
