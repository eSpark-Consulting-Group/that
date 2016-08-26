using DDT.Application.Models;
using System;
using System.Threading.Tasks;

namespace DDT.Application.Services
{
    public interface IReportService
    {
        Task<Tuple<string, string>> BuildDiscoveryAndDeploymentReport(DeploymentItem[] targetsByIp, DeploymentItem[] targetsByDomain, DeploymentItem[] targetsByWorkgroup, Guid operationUid);    
    }
}