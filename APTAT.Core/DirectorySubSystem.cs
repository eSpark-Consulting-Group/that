using System;
using System.Net;
using System.Net.NetworkInformation;

namespace APTAT.Core
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

        public virtual void SendPingAsync(string hostName, Action<IPStatus, IPAddress> completedCallback)
        {
            NetHelp.SendPingAsync(hostName, completedCallback);
        }

        public virtual bool TestMsDs(string hostName)
        {
            return NetHelp.TestMsDs(hostName);
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
    }
}
