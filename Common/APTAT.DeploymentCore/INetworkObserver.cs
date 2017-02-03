using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using APTAT.Common;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.DeploymentCore
{
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

        string GetLocalHostName(bool b);
        string[] GetSupportedExternalAddresses(bool b);
        string CutDnsSuffixFromHostName(string oldRemoteHostName);
        IPAddress[] SafeResolveDns(string hostName);
        bool IsValidDnsName(string hostName);
        bool IsValidIpAddress(string hostName);
        IPStatus SendPing(string toString);
        IPHostEntry GetHostEntry();
        bool IsLoopbackAddress(string nameOrAddress);
        void AutoResolveByNetworkSubnet(CancellationTokenSource ts);
        event EventHandler<DeploymentTargetResolvingEventArg> DeploymentTargetResolving;
        List<Tuple<IPAddress, IPAddress>> GetLocalAddressRange();
        string GetCurrentWorkgroupName();
    }
}