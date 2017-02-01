using System;
using APTAT.DeploymentCore.Facade;

namespace APTAT.DeploymentCore
{
    public interface IComputer
    {
        Guid Uid { get; set; }
        string Name { get; set; }
        string DnsName { get; set; }
        string IPAddress { get; set; }
        bool IsOnline { get; set; }
        string OrganizationUnit { get; }
        string LdapPath { get; set; }
        SystemKind? SystemKind { get; set; }
        Boolean IsLocalComputer { get; }
        DeploymentTargetSource Source { get; set; }
    }
}
