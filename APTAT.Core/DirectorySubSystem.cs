using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace APTAT.Core
{
    public interface IComputer
    {
        Guid Uid { get; set; }
        string Name { get; set; }
        string DnsName { get; set; }
        Guid OriginalAgentIdentityOnConnect { get; set; }
        IPAddress IPAddress { get; set; }
        bool IsOnline { get; set; }
        string OrganizationUnit { get; set; }
        string LdapPath { get; set; }
    }

    public class DirectoryComputer : IComputer
    {
        public Guid Uid { get; set; }
        public string Name { get; set; }
        public string DnsName { get; set; }
        public Guid OriginalAgentIdentityOnConnect { get; set; }
        public IPAddress IPAddress { get; set; }
        public bool IsOnline { get; set; }
        public string OrganizationUnit { get; set; }
        public string LdapPath { get; set; }
    }

    public interface IDirectorySubSystem
    {
        IActiveDirectoryObserver ActiveDirectoryObserver { get; }
    }

    public class DirectorySubSystem : IDirectorySubSystem, INetworkObserver
    {
        public IActiveDirectoryObserver ActiveDirectoryObserver { get; set; }

        #region INetworkObserver Members

        public virtual IPAddress[] ResolveIpByDns(string dnsName)
        {
            return NetHelp.SafeResolveDns(dnsName);
        }

        public virtual string ResolveDnsByIp(IPAddress address)
        {
            return NetHelp.SafeResolveIp(address);
        }

        public virtual IPStatus SendPing(string hostName)
        {
            return NetHelp.SendPing(hostName);
        }

        public virtual void SendPingAsync(string hostName, Action<IPStatus, IPAddress> completedCallback)
        {
            NetHelp.SendPingAsync(hostName, completedCallback);
        }

        public virtual bool TestMsDs(string hostName)
        {
            return NetHelp.TestMsDs(hostName);
        }

        public virtual void TestMsDsAsync(string hostName, Action<bool, IPAddress> completedCallback)
        {
            NetHelp.TestMsDsAsync(hostName, completedCallback);
        }

        public virtual string[] EnumerateWorkgroups()
        {
            return NetHelp.EnumerateWorkgroups();
        }

        public virtual IPAddress GetInterNetworkIpAddress(IEnumerable<IPAddress> addressList)
        {
            return NetHelp.GetInterNetworkIpAddress(addressList);
        }

        public virtual bool IsValidWorkgroupName(string name)
        {
            return NetworkBrowser.IsWorkgroupName(name);
        }

        #endregion
    }

    public interface IService //marker
    {
    }
    

    
    public interface INetworkObserver : IService
    {
        /// <summary>
        /// Resolves the specified dnsName to Computer object.
        /// </summary>
        /// <param name="dnsName">The Dns name.</param>
        /// <returns>Computer object</returns>
        IPAddress[] ResolveIpByDns(string dnsName);

        /// <summary>
        /// Resolves the specified DnsName to Computer object.
        /// </summary>
        /// <param name="address">The IP Address.</param>
        /// <returns>Computer object</returns>
        string ResolveDnsByIp(IPAddress address);

        /// <summary>
        /// Sending ping to specified hostName
        /// </summary>
        /// <param name="hostName">Computername or ip of computer to ping </param>
        /// <returns>Ping status</returns>
        IPStatus SendPing(string hostName);

        /// <summary>
        /// Sending ping to specified hostName in async manner
        /// </summary>
        /// <param name="hostName">Computername or ip of computer to ping </param>
        /// <param name="completedCallback">Callback which will be called upon ping completion</param>
        void SendPingAsync(string hostName, Action<IPStatus, IPAddress> completedCallback);

        /// <summary>
        /// Attempts TCP connection to port 445 of specified hostName
        /// </summary>
        /// <param name="hostName">Computer name or IP</param>
        /// <returns>True if connection succeeded</returns>
        bool TestMsDs(string hostName);

        /// <summary>
        /// Attempts TCP connection to port 445 of specified hostName (async version)
        /// </summary>
        /// <param name="hostName">Computer name or IP</param>
        /// <param name="completedCallback">Callback method to call when test is completed</param>
        void TestMsDsAsync(string hostName, Action<bool, IPAddress> completedCallback);

        /// <summary>
        /// Retrieving available workgroups 
        /// </summary>
        /// <returns>Array, that contains workgroup names</returns>
        string[] EnumerateWorkgroups();

        /// <summary>
        /// Finding InterNetwork ip address
        /// </summary>
        /// <param name="addressList"></param>
        /// <returns></returns>
        IPAddress GetInterNetworkIpAddress(IEnumerable<IPAddress> addressList);

        /// <summary>
        /// Validating given name with workgroup naming rules
        /// </summary>
        /// <param name="name">Name to validate</param>
        /// <returns>True - if name is valid, false otherwise</returns>
        bool IsValidWorkgroupName(string name);
    }
}
