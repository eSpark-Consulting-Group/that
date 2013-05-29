using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Core
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
        string Name { get; }
        string DnsName { get; }
        AdEntryType Type { get; }
    }

    public enum AdEntryType
    {
        AdRoot = 0,
        OrganizationUnit = 1,
        Computer = 2,
    }
}
