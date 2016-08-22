using System;
using APTAT.Common;

namespace APTAT.DeploymentCore
{
    public interface IActiveDirectoryObserver : IService
    {
        IAdEntry GetDomainRootEntry(string domainName, string login, string password);

        IAdEntry[] GetChildEntries(string aDsPath, string login, string password);
        IAdEntry[] GetChildEntries(string aDsPath, string objectClass, string login, string password);
    }

    public interface IAdEntry
    {
        string FullPath { get; }
        string DomainName { get; }
        string Name { get; }
        string DnsName { get; }
        AdEntryType Type { get; }
        string OsName { get; }
        string OsVersion { get; }
        string ServicePack { get; }
        string Sid { get; }
        DateTime LastLogon { get; }
        DateTime LastLogonTimestamp { get; }
    }

    public enum AdEntryType
    {
        AdRoot = 0,
        OrganizationUnit = 1,
        Computer = 2,
    }
}
