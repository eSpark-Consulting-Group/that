using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using APTAT.Core.Descent;

namespace APTAT.Core.Directory
{
    public class ActiveDirectoryObserver : MarshalByRefNullLifetime, IActiveDirectoryObserver
    {
        public IAdEntry GetDomainRootEntry(string domainName, string login, string password)
        {
            using (DirectoryEntry root = CreateDomainDirectoryEntry(domainName, login, password))
            {
                AdEntry entry;
                DateTime start = DateTime.UtcNow;
                try
                {
                    entry = new AdEntry(root.Name, root.Path, AdEntryType.AdRoot, null);
                    LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Network,
                        "Obtained domain '{1}' root path: {2}{0} elapsed: {3}{0}",
                        Environment.NewLine, root.Name, root.Path, DateTime.UtcNow - start);
                }
                catch (Exception ex)
                {
                    LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Network,
                        "Error querying domain '{1}' root{0} elapsed: {2}{0} exception: {3}{0}",
                        Environment.NewLine, domainName, DateTime.UtcNow - start, ex);
                    throw new DirectoryQueryException("Error querying domain root.", ex, domainName, DirectoryError.SpecifiedDomainNotFound);
                }

                return entry;
            }
        }

        public IAdEntry[] GetChildEntries(string aDsPath, string login, string password)
        {
            List<IAdEntry> result = new List<IAdEntry>();

            using (DirectoryEntry searchRoot = CreateDirectoryEntry(aDsPath, login, password))
            {
                var ouCollection = FindEntries(searchRoot, AdEntryType.OrganizationUnit, "(objectClass=organizationalUnit)");
                result.AddRange(ouCollection);

                var compCollection = FindEntries(searchRoot, AdEntryType.Computer, "(objectClass=computer)");
                result.AddRange(compCollection);
            }

            return result.ToArray();
        }

        public IAdEntry[] GetChildEntries(string aDsPath, string objectClass, string login, string password)
        {
            using (DirectoryEntry searchRoot = CreateDirectoryEntry(aDsPath, login, password))
            {
                return FindEntries(searchRoot, AdEntryType.OrganizationUnit, String.Format("(objectClass={0})", objectClass)).ToArray();
            }
        }

        private static DirectoryEntry CreateDomainDirectoryEntry(string domainName, string login, string password)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                try
                {
                    using (var entry = CreateDirectoryEntry(@"LDAP://RootDSE", login, password))
                    {
                        domainName = entry.Properties["defaultNamingContext"][0].ToString();
                    }
                }
                catch (COMException ex)
                {
                    // HRESULT is 32 bit value, so possibility of the highest sign bit of Int32 misunderstainding exists.
                    // So first converting to Int64 - possible negative bit is saved.
                    // Then safely casting to uint without loosing any bits.
                    uint code = (uint)(long)ex.ErrorCode;
                    if (code == 0x8007054B)
                    {
                        // Domain not found: "The specified domain either does not exist or could not be contacted."
                        LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Network,
                            "The server is not in domain or could not be contacted under current credentials. Exception: {0}", ex);
                        throw new DirectoryQueryException("The server is not in domain or could not be contacted under current credentials.",
                            ex, String.Empty, DirectoryError.DefaultDomainNotFound);
                    }
                    else
                    {
                        LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network, "Unknown COMException occurred while accessing default root: {0}", ex);
                        throw new DirectoryQueryException("Unknown domain query error occurred.", ex, String.Empty);
                    }
                }
            }

            string domainPath = string.Format(@"LDAP://{0}", domainName);
            DirectoryEntry domain = CreateDirectoryEntry(domainPath, login, password);

            return domain;
        }


        private static DirectoryEntry CreateDirectoryEntry(string aDsPath, string login, string password)
        {
            DirectoryEntry searchRoot = new DirectoryEntry(
                aDsPath,
                login,
                password,
                AuthenticationTypes.Secure | AuthenticationTypes.FastBind
                );
            return searchRoot;
        }


        private static IEnumerable<IAdEntry> FindEntries(DirectoryEntry searchRoot, AdEntryType type, string filter)
        {
            DirectorySearcher searcher = new DirectorySearcher(
                    searchRoot,
                    filter,
                    new string[] {/* "Name"*/ },
                    SearchScope.OneLevel
                    );

            using (searcher)
            {
                DateTime start = DateTime.UtcNow;
                SearchResultCollection src;
                try
                {
                    src = searcher.FindAll();
                }
                catch (InvalidOperationException ex)
                {
                    string message = String.Format("The specified directory entry with path '{0}' and filter '{1}' is not a container. Exception: {2}", searchRoot.Path, filter, ex);
                    LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network, message);
                    throw new DirectoryQueryException(message, ex, String.Empty, DirectoryError.ParentEntryIsNotContainer);
                }

                using (src)
                {
                    LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network,
                        "AD query finished{0} parent: {1}{0} filter: '{2}'{0} elapsed: {3}{0}",
                        Environment.NewLine, searchRoot.Path, filter, DateTime.UtcNow - start);
                    try
                    {
                        List<IAdEntry> entries = new List<IAdEntry>();
                        foreach (SearchResult sr in src)
                        {
                            var dnsProperties = sr.Properties["DNSHostName"];
                            string dnsName = null;
                            if (dnsProperties != null && dnsProperties.Count > 0)
                            {
                                dnsName = dnsProperties[0].ToString();
                            }

                            IAdEntry entry = new AdEntry(sr.Properties["Name"][0].ToString(), sr.Properties["ADsPath"][0].ToString(), type, dnsName);
                            entries.Add(entry);
                        }

                        return entries;
                    }
                    catch (DirectoryServicesCOMException ex)
                    {
                        string message = String.Format("Error enumerating children from path '{0}' and filter '{1}'. Exception: {2}", searchRoot.Path, filter, ex);
                        LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network, message);
                        throw new DirectoryQueryException(message, ex, String.Empty, DirectoryError.QueryError);
                    }
                }
            }
        }

        [Serializable]
        private class AdEntry : IAdEntry
        {
            public AdEntry(string name, string fullPath, AdEntryType type, string dnsName)
            {
                Name = name;
                DnsName = dnsName;
                FullPath = fullPath;
                Type = type;
            }

            public string Name { get; set; }
            public string DnsName { get; set; }
            public string FullPath { get; set; }
            public AdEntryType Type { get; set; }
        }
    }

    public class DirectoryQueryException : InvalidOperationException
    {
        public DirectoryQueryException(string message, Exception innerException, string domainName, DirectoryError error = DirectoryError.Unknown)
            : base(message, innerException)
        {
            Path = domainName;
            Error = error;
        }

        public string Path
        {
            get;
            set;
        }

        public DirectoryError Error
        {
            get;
            set;
        }
    }

    public enum DirectoryError
    {
        Unknown = 0,
        DefaultDomainNotFound,
        SpecifiedDomainNotFound,
        QueryError,
        ParentEntryIsNotContainer,
    }
}
