using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Properties;

namespace APTAT.DeploymentCore
{
    public class NetworkObserver : INetworkObserver
    {

        #region INetworkObserver Members

        public IPHostEntry GetHostEntry()
        {
            return Dns.GetHostEntry(String.Empty);
        }
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

        public string GetLocalHostName(bool fullName)
        {
            return NetHelp.GetLocalHostName(fullName);
        }

        public string[] GetSupportedExternalAddresses(bool includeDnsIpAddresses)
        {
            return NetHelp.GetSupportedExternalAddresses(includeDnsIpAddresses);
        }

        public string CutDnsSuffixFromHostName(string oldRemoteHostName)
        {
            return NetHelp.CutDnsSuffixFromHostName(oldRemoteHostName);
        }

        public IPAddress[] SafeResolveDns(string hostName)
        {
            return NetHelp.SafeResolveDns(hostName);
        }

        public bool IsValidDnsName(string hostName)
        {
            return NetHelp.IsValidDnsName(hostName);
        }

        public bool IsValidIpAddress(string hostName)
        {
            return NetHelp.IsValidIpAddress(hostName);
        }

        public IPStatus SendPing(string toString)
        {
            return NetHelp.SendPing(toString);
        }

        public bool IsLoopbackAddress(string nameOrAddress)
        {
            return NetHelp.IsLoopbackAddress(nameOrAddress);
        }

        //public IPAddress[] GetLocalAddressRanges()
        //{
        //    string hostName = Dns.GetHostName();
        //    IPHostEntry iphostentry = Dns.GetHostEntry(hostName);
        //    var ips = iphostentry.AddressList.Where(i => i.IsIPv6LinkLocal == false).ToArray();
        //    return ips;
        //}

        private static uint IPAddressToEnumerableInt(IPAddress ip)
        {
            byte[] ipBytes = ip.GetAddressBytes();
            Array.Reverse(ipBytes);
            return BitConverter.ToUInt32(ipBytes, 0);
        }

        private static IPAddress EnumerableIntToIPAddress(uint addr)
        {
            byte[] ipBytes = BitConverter.GetBytes(addr);
            Array.Reverse(ipBytes);
            return new IPAddress(ipBytes);
        }

        private List<IPAddress> GetComputersInRange(IPAddress from, IPAddress to)
        {
            List<IPAddress> retValue = new List<IPAddress>();

            UInt32 fromAddress = IPAddressToEnumerableInt(from);
            UInt32 toAddress = IPAddressToEnumerableInt(to);

            if (fromAddress > toAddress)
            {
                UInt32 tmp = fromAddress;
                fromAddress = toAddress;
                toAddress = tmp;
            }

            for (UInt32 addr = fromAddress; addr <= toAddress; addr++)
            {
                IPAddress ipAddress = EnumerableIntToIPAddress(addr);
                retValue.Add(ipAddress);
            }
            return retValue;
        }
        
        private EndpointTarget CheckTarget(IPAddress ipAddress, string[] serverAddresses, string serverDnsName)
        {
            var status = SendPing(ipAddress.ToString());
            if (status != IPStatus.Success) return null;
            
            var target = new EndpointTarget
            {
                Uid = Guid.NewGuid(),
                Name = ipAddress.ToString(),
                IPAddress = ipAddress.ToString()
            };

            if (target.IPAddress != null)
            {
                target.DnsName = ResolveDnsByIp(IPAddress.Parse(target.IPAddress));
                target.IsOnline = TestMsDs(target.IPAddress);

                target.IsLocalComputer = serverAddresses.Any(ip => ip == target.IPAddress) || string.Equals(target.DnsName, serverDnsName, StringComparison.InvariantCultureIgnoreCase);
            }
           
            return target;
        }

        public event EventHandler<DeploymentTargetResolvingEventArg> DeploymentTargetResolving;

        public List<Tuple<IPAddress, IPAddress>> GetLocalAddressRange()
        {
            return AddressRangeHelper.GetLocalAddressRange();
        }

        public string GetCurrentWorkgroupName()
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher(@"root\CIMV2", @"SELECT * FROM Win32_ComputerSystem");

            foreach (ManagementObject mo in mos.Get())
            {
                var obj = mo["Workgroup"];
                if (obj != null)
                {
                    return obj.ToString();
                }
            }
            return null;
        }

        public void AutoResolveByNetworkSubnet(CancellationTokenSource cts)
        {
            List<Tuple<IPAddress, IPAddress>> subnetRanges = AddressRangeHelper.GetLocalAddressRange();
            
            List<IPAddress> ipAddresses = new List<IPAddress>();
            foreach (var range in subnetRanges)
            {
                var fromAndToIps = NetHelp.GetIpAddressMinMax(range.Item1, range.Item2);
                var computersInRange = GetComputersInRange(fromAndToIps.Item1, fromAndToIps.Item2);
                ipAddresses.AddRange(computersInRange);
            }


            int computersCount = ipAddresses.Count;
            int processed = 0;

            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = cts.Token;
            po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
            var serverDnsName = GetLocalHostName(true);
            var serverAddresses = GetSupportedExternalAddresses(true);

            double progress = 0;
            Action<IPAddress> action = (r) => {
                po.CancellationToken.ThrowIfCancellationRequested();
                var target = CheckTarget(r, serverAddresses, serverDnsName);

                Interlocked.Increment(ref processed);
                lock (lockObject)
                {
                    if (computersCount == 0)
                    {
                        progress = 100;
                    }
                    else
                    {
                        progress = processed / (double) computersCount;
                    }
                    OnDeploymentTargetResolving(new DeploymentTargetResolvingEventArg(target, progress));
                }
            };
            try
            {
                Parallel.ForEach(ipAddresses, po, action);
            }
            catch (OperationCanceledException e)
            {
                LogHelper.Trace(TraceEventType.Information, e.Message);
            }
            finally
            {
                cts.Dispose();
            }

        }

       

        #endregion

        #region NetHelper
        private static class NetHelp
        {
            private const int MICROSOFT_DS = 445;
            private const int NETBIOS_SSN = 139;

            public static string GetLocalHostName(bool fullName)
            {
                string hostName;
                if (!fullName)
                {
                    hostName = Dns.GetHostName();
                }
                else
                {
                    // Trying to get full host name with DNS suffix from DNS server using Dns.GetHostEntry() 
                    // instead of the only host name given by Dns.GetHostName().
                    IPHostEntry hostEntry = Dns.GetHostEntry(String.Empty);
                    hostName = hostEntry.HostName;
                }

                return hostName;
            }

            /// <summary>
            /// Gets one ore more supported external addresses from the config file. Returns current DNS name if the config file does not contain external addresses.     
            /// </summary>
            /// <param name="includeDnsIpAddresses">if true all the possible IP addresses associated with the current host in DNS will be uncluded in the result.</param>
            public static string[] GetSupportedExternalAddresses(bool includeDnsIpAddresses)
            {
                IEnumerable<string> dnsNames = Settings.Default.SupportedDnsNamesList;

                if (includeDnsIpAddresses)
                {
                    // The following block extracts all the possible IP addresses associated with the current host in DNS.
                    IPHostEntry ipHostEntry = Dns.GetHostEntry(String.Empty);
                    dnsNames = dnsNames.Union(from address in ipHostEntry.AddressList
                                              where address.AddressFamily == AddressFamily.InterNetwork
                                              select address.ToString());
                }

                dnsNames = dnsNames.Where(name => !IsLoopbackAddress(name));

                var dnsNamesList = dnsNames as IList<string> ?? dnsNames.ToList();
                if (!dnsNamesList.Any())
                {
                    throw new InvalidOperationException(String.Format("The setting 'SupportedDnsNames' in the application configuration file should contain at least one external (not loopback) address."));
                }

                return dnsNamesList.ToArray();
            }



            /*
            private static readonly VolatileCache<IPAddress, string> s_dnsCache = new VolatileCache<IPAddress, string>(TimeSpan.FromMinutes(1));

            // Get DNS name by IP address
            public static string SafeResolveIp( IPAddress address )
            {
                try
                {
                    var hostName = s_dnsCache[address];
                    if (hostName != null)
                    {
                        Debug.WriteLine(String.Format("DNS from cache for IP{0} address: {1}{0} DNS: {2}{0}", Environment.NewLine, address, hostName), "DNS");
                    }
                    else
                    {
                        hostName = Dns.GetHostEntry(address).HostName;
                        Debug.WriteLine(String.Format("DNS resolved by IP{0} address: {1}{0} DNS: {2}{0}", Environment.NewLine, address, hostName), "DNS");
                        s_dnsCache[address] = hostName;
                    }
                    return hostName;
                }
                catch( Exception ex )
                {
                    Trace.TraceWarning( "DNS failed to be resolved by IP{0} address: {1}{0} exception: {2}{0}", Environment.NewLine, address, ex.Message );
                    return address.ToString();
                }
            }
         */

            // Get DNS name by IP address
            public static string SafeResolveIp(IPAddress address)
            {
                try
                {
                    string hostName = Dns.GetHostEntry(address).HostName;
                    LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Network,
                        "DNS resolved by IP{0} address: {1}{0} DNS: {2}{0}", Environment.NewLine, address, hostName);
                    return hostName;
                }
                catch (Exception ex)
                {
                    LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network,
                        "DNS failed to be resolved by IP{0} address: {1}{0} exception: {2}{0}", Environment.NewLine, address, ex.Message);
                    return address.ToString();
                }
            }

            // Compare two host names
            public static bool AreHostNamesEqual(string host1, string host2)
            {
                if (string.Compare(host1, host2, true) == 0)
                    return true;

                IPAddress[] ips1 = SafeResolveDns(host1);
                IPAddress[] ips2 = SafeResolveDns(host2);

                if (ips1 != null && ips2 != null)
                {
                    for (int i = 0; i < ips1.Length; ++i)
                    {
                        if (Array.IndexOf(ips2, ips1[i]) != -1)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            // Get IP address by DNS name
            public static IPAddress[] SafeResolveDns(string dns)
            {
                if (string.IsNullOrEmpty(dns))
                    return null;

                try
                {
                    IPHostEntry host = Dns.GetHostEntry(dns);
                    var ipArray = host != null ? host.AddressList : null;

                    var list = String.Join(";", ipArray ?? Enumerable.Empty<IPAddress>());
                    LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Network,
                        "IP address resolved by DNS{0} name: {1}{0} address: {2}{0}", Environment.NewLine, dns, list);

                    return ipArray;
                }
                catch (Exception x)
                {
                    LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network,
                        "IP address failed to resolve by DNS{0} name: {1}{0} exception: {2}{0}", Environment.NewLine, dns, x.Message);
                    return null;
                }
            }

            /// <summary>
            /// Extract pure host dns name without any suffixes
            /// </summary>
            /// <param name="hostName"></param>
            /// <returns></returns>
            public static string CutDnsSuffixFromHostName(string hostName)
            {
                int i = hostName.IndexOf('.');
                if (i != -1)
                {
                    string newHostName = hostName.Substring(0, i);

                    // Check if dns name is not a number (like if it was IP address in the first place)
                    int intValue;
                    if (!Int32.TryParse(newHostName, out intValue))
                    {
                        hostName = newHostName;
                    }
                }

                return hostName;
            }

            // Send ping and get IPStatus
            public static IPStatus SendPing(string hostName)
            {
                var status = IPStatus.Unknown;

                using (var resetEvent = new ManualResetEventSlim(false))
                {
                    SendPingAsync(hostName, (pingRes, ipAddress) =>
                    {
                        status = pingRes;
                        resetEvent.Set();
                    });
                    resetEvent.Wait();
                }

                return status;
            }

            public static void SendPingAsync(string hostName, Action<IPStatus, IPAddress> completedCallback)
            {
                const int timeout = 120;

                Ping pingSender = new Ping();
                pingSender.PingCompleted += (sender, e) =>
                {
                    if (completedCallback != null)
                    {
                        if (e.Reply != null)
                        {
                            LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Network, "Ping completed from machine '{0}' Status: {1}", hostName, e.Reply.Status);
                            completedCallback(e.Reply.Status, e.Reply.Address);
                        }
                        else
                        {
                            LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Network, "Ping was not completed from machine '{0}' Error: {1}", hostName, e.Error);
                            completedCallback(IPStatus.Unknown, null);
                        }
                    }

                    pingSender.Dispose();
                };

                PingOptions options = new PingOptions();

                // Use the default Ttl value which is 128,
                // but change the fragmentation behavior.
                options.DontFragment = true;

                // Create a buffer of 32 bytes of data to be transmitted.
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                try
                {
                    pingSender.SendAsync(hostName, timeout, buffer, options, pingSender);
                }
                catch (Exception ex)
                {
                    LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Network, "Ping failed for machine '{0}' Error: {1}", hostName, ex);
                    if (completedCallback != null)
                    {
                        completedCallback(IPStatus.Unknown, null);
                    }
                }
            }

            public static bool TestMsDs(string hostName)
            {
                var result = false;

                using (var msdsEvent = new ManualResetEvent(false))
                {
                    TestMsDsAsync(hostName, (msdsResult, ipAddress) =>
                    {
                        result = msdsResult;
                        msdsEvent.Set();
                    });

                    msdsEvent.WaitOne();
                }

                return result;
            }

            public static void TestMsDsAsync(string hostName, Action<bool, IPAddress> completionCallback)
            {
                var client = new TcpClient(AddressFamily.InterNetwork);
                var callback = new AsyncCallback[] { null };

                Action<int> beginConnect = port =>
                {
                    IPAddress address;
                    if (IPAddress.TryParse(hostName, out address))
                        client.BeginConnect(address, port, callback[0], port);
                    else
                        client.BeginConnect(hostName, port, callback[0], port);
                };

                callback[0] = ar =>
                {
                    var port = (int) ar.AsyncState;
                    try
                    {
                        client.EndConnect(ar);
                        var remote = (IPEndPoint) client.Client.RemoteEndPoint;
                        LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Network, "Connection to {0}:{1} succeeded.", hostName, remote.Port);
                        client.Close();
                        completionCallback(true, remote.Address);
                    }
                    catch (Exception x)
                    {
                        LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Network, "Connection to {0}:{1} failed: {2}", hostName, port, x.Message);
                        if (port == MICROSOFT_DS)
                        {
                            beginConnect(NETBIOS_SSN);
                        }
                        else
                        {
                            client.Close();
                            completionCallback(false, null);
                        }
                    }
                };

                // Refactored to sequental execution due to shortage of threads in a thread pool,
                // which causes WCF calls from Console to fail with timeout.
                beginConnect(MICROSOFT_DS);
            }

            public static IPAddress GetInterNetworkIpAddress(IEnumerable<IPAddress> addressList)
            {
                if (addressList != null)
                {
                    foreach (IPAddress address in addressList)
                    {
                        if (address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return address;
                        }
                    }
                }
                return null;
            }

            public static bool IsValidIpAddress(string address)
            {
                if (address == null) throw new ArgumentNullException("address");

                const string ipAddressPattern = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
                return Regex.IsMatch(address, ipAddressPattern);
            }

            public static bool IsValidDnsName(string address)
            {
                if (address == null) throw new ArgumentNullException("address");

                const string atLeastOneLetterPattern = @"[A-Za-z]+"; // to avoid input like '111' or '999.999'
                const string dnsNamePattern = @"^(?=.{1,255}$)[0-9A-Za-z](?:(?:[0-9A-Za-z]|\b-){0,61}[0-9A-Za-z])?(?:\.[0-9A-Za-z](?:(?:[0-9A-Za-z]|\b-){0,61}[0-9A-Za-z])?)*$";

                return address.Length < 256 // checks total lenght
                       && Regex.IsMatch(address, atLeastOneLetterPattern) && Regex.IsMatch(address, dnsNamePattern);
            }

            public static bool IsLoopbackAddress(string nameOrAddress)
            {
                if (nameOrAddress == null) throw new ArgumentNullException("nameOrAddress");
                IPAddress ip;
                bool isLoopback = IPAddress.TryParse(nameOrAddress, out ip)
                    ? IPAddress.IsLoopback(ip)
                    : "localhost" == nameOrAddress.ToLowerInvariant();
                return isLoopback;
            }

            public static bool IsLoopbackAddress(IPAddress address)
            {
                return IPAddress.IsLoopback(address);
            }

            public static Tuple<IPAddress, IPAddress> GetIpAddressMinMax(IPAddress ip, IPAddress mask)
            {
                byte[] ipBytes = ip.GetAddressBytes();
                byte[] maskBytes = mask.GetAddressBytes();

                byte[] ipFromBytes = new byte[4];
                byte[] ipToBytes = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    ipFromBytes[i] = (byte) (ipBytes[i] & maskBytes[i]);
                    ipToBytes[i] = (byte) (ipBytes[i] | ~maskBytes[i]);
                }

                var from = new IPAddress(ipFromBytes);
                var to = new IPAddress(ipToBytes);

                return new Tuple<IPAddress, IPAddress>(from, to);

            }
        }
        #endregion
        private static object lockObject = new object();

        protected virtual void OnDeploymentTargetResolving(DeploymentTargetResolvingEventArg e)
        {
            lock (lockObject)
            {
                DeploymentTargetResolving?.Invoke(this, e);
            }
        }
    }
}