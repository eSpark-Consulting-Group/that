using DDT.Application.Models;
using System;
using System.Threading.Tasks;

namespace DDT.Application.Services
{
    public interface IReportService
    {
        Task<string> BuildDiscoveryAndDeploymentReport(DeploymentItem[] targetsByIp, DeploymentItem[] targetsByDomain, DeploymentItem[] targetsByWorkgroup, Guid operationUid);    
    }
}