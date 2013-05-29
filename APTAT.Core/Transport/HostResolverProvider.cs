using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Core;
using APTAT.Core.Properties;
using CRM.SrvCore.Descent;
using APTAT.Core.Descent;

namespace APTAT.Core.Transport
{
    public interface IHostResolverProvider
    {
        IHostResolver GetResolver(IEnumerable<IPAddress> addresses, INetworkObserver networkObserver);
    }

    public class HostResolverProvider : IHostResolverProvider
    {
        public IHostResolver GetResolver(IEnumerable<IPAddress> addresses, INetworkObserver networkObserver)
        {
            return new HostResolver(addresses, networkObserver);
        }
    }

    public class HostResolver : IHostResolver
    {
        private List<ResolveContext> _resolveWorkItems = new List<ResolveContext>();
        private INetworkObserver _networkObserver;
        private object _resolveFinishedSync = new object();
        private int _itemsInProcessCount;
        public event EventHandler ResolveFinished;

        protected virtual void OnResolveFinished()
        {
            var temp = ResolveFinished;
            if (temp != null)
            {
                ResolveFinished(this, EventArgs.Empty);
            }
        }

        public List<ResolveContext> ResolveWorkItems
        {
            get { return _resolveWorkItems; }
            set { _resolveWorkItems = value; }
        }

        public HostResolver(IEnumerable<IPAddress> hostList, INetworkObserver networkObserver)
        {
            _networkObserver = networkObserver;
            foreach (IPAddress host in hostList)
            {
                _resolveWorkItems.Add(PrepareResolveContext(host));
                _itemsInProcessCount++;
            }
        }

        public void Start()
        {
            foreach (ResolveContext context in ResolveWorkItems)
            {
                context.StartResolve();
            }
        }

        private void ResolveProcessCallback(ResolveContext ioContext)
        {
            // Resolve Dns
            IPHostEntry ips = new IPHostEntry();
            ips.Aliases = new string[] { ioContext.HostName };
            ips.AddressList = _networkObserver.ResolveIpByDns(ioContext.HostName);
            if (ips.AddressList == null || ips.AddressList.Length == 0)
            {
                ips.AddressList = new[] { ioContext.IPAddress };
            }
            IPAddress addr;
            if (IPAddress.TryParse(ioContext.HostName, out addr))
            {
                ips.HostName = _networkObserver.ResolveDnsByIp(addr);
            }
            else
            {
                ips.HostName = ioContext.HostName;
            }

            ioContext.PublishDnsResolveResult(ips);

            //	IPStatus reply = _networkObserver.SendPing( ioContext.IPAddress.ToString() );
            //	ioContext.PublishPingReply( reply );

            var result = _networkObserver.TestMsDs(ioContext.IPAddress.ToString());
            ioContext.PublishPingReply(result ? IPStatus.Success : IPStatus.Unknown);

            lock (_resolveFinishedSync)
            {
                _itemsInProcessCount--;
                if (_itemsInProcessCount == 0)
                {
                    OnResolveFinished();
                }
            }
        }

        public ResolveContext PrepareResolveContext(IPAddress ip)
        {
            ResolveContext context = new ResolveContext(ip, ResolveProcessCallback);
            return context;
        }
    }

    public interface IHostResolver
    {
        event EventHandler ResolveFinished;
        List<ResolveContext> ResolveWorkItems { get; set; }
        void Start();
    }

    public delegate void ResolveProcessDelegate(ResolveContext ioContext);

    public class ResolveContext
    {
        private static TaskFactory s_taskFactory;

        private IPAddress _ip;
        private string _hostName;
        private DnsResolveState _dnsResolveState;
        private IPStatus _pingReplayStatus;
        private ResolveProcessDelegate _resolveStartMethod;
        private ResolveState _state;// = ResolveState.Pending;

        //public event EventHandler DnsResolveFinished;
        //public event EventHandler PingReplayRetrieved;
        //public event EventHandler StateChanged;

        static ResolveContext()
        {
            s_taskFactory = new TaskFactory(
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskContinuationOptions.None,
                new LimitedConcurrencyLevelTaskScheduler(Settings.Default.DescentMaximumConcurrencyLevel));
        }

        public ResolveContext(IPAddress ip, ResolveProcessDelegate resolveStartMethod)
        {
            _ip = ip;
            _hostName = ip.ToString();
            _resolveStartMethod = resolveStartMethod;
            _dnsResolveState = new DnsResolveState(ip.ToString(), null);

            _state = ResolveState.Pending;
            _pingReplayStatus = IPStatus.Unknown;
        }

        private void ResolveThreadStartCallback(object context)
        {
            _resolveStartMethod((ResolveContext)context);
        }

        public void StartResolve()
        {
            s_taskFactory.StartNew(ResolveThreadStartCallback, this);
        }

        #region Publish results

        public void PublishDnsResolveResult(IPHostEntry IPs)
        {
            if (IPs != null)
            {
                _dnsResolveState = new DnsResolveState(_dnsResolveState.Host, IPs);
                //OnDnsResolveFinished();
                _state = ResolveState.DnsResolved;
                _hostName = IPs.HostName;
            }
            else
            {
                _state = ResolveState.DnsFailed;
            }
            //OnStateChanged();
        }

        public void PublishPingReply(IPStatus replayStatus)
        {
            _pingReplayStatus = replayStatus;
            //OnPingReplayRetrieved();
            if (replayStatus == IPStatus.Success)
            {
                _state = ResolveState.PingSuccess;
            }
            else
            {
                _state = ResolveState.PingFailed;
            }
            //OnStateChanged();
        }

        #endregion

        //#region Raise events

        //protected virtual void OnDnsResolveFinished()
        //{
        //    if( DnsResolveFinished != null )
        //    {
        //        DnsResolveFinished( this, EventArgs.Empty );
        //    }
        //}

        //protected virtual void OnPingReplayRetrieved()
        //{
        //    if( PingReplayRetrieved != null )
        //    {
        //        PingReplayRetrieved( this, EventArgs.Empty );
        //    }
        //}

        //protected virtual void OnStateChanged()
        //{
        //    if( StateChanged != null )
        //    {
        //        StateChanged( this, EventArgs.Empty );
        //    }
        //}
        //#endregion

        #region Public properties

        public string HostName
        {
            get { return _hostName; }
        }

        public IPAddress IPAddress
        {
            get { return _ip; }
        }

        public ResolveState State
        {
            get { return _state; }
        }

        public IPStatus PingReplayStatus
        {
            get { return _pingReplayStatus; }
            set { _pingReplayStatus = value; }
        }

        public DnsResolveState DnsResolveState
        {
            get { return _dnsResolveState; }
        }

        #endregion

    }

    public enum ResolveState
    {
        Pending = 0,
        DnsResolved,
        DnsFailed,
        PingSuccess,
        PingFailed
    }

    public struct DnsResolveState
    {
        private readonly string _hostName;
        private readonly IPHostEntry _resolvedIPs;

        public DnsResolveState(string hostName, IPHostEntry hostEntry)
        {
            _hostName = hostName;
            _resolvedIPs = hostEntry;
        }

        public IPHostEntry IPs
        {
            get { return _resolvedIPs; }
        }

        public string Host
        {
            get { return _hostName; }
        }

    }

    public interface IIPSelectionStrategy
    {
        IEnumerable<IPAddress> GetIPs();
    }

    public class InvalidAddressException : ArgumentException
    {
        public InvalidAddressException(string message, string paramName, InvalidityReason reason)
            : base(message, paramName)
        {
            Reason = reason;
        }

        public InvalidityReason Reason
        {
            get;
            set;
        }
    }

    public enum InvalidityReason
    {
        InvalidSubnetMask,
        TooManyAddressesInRange,
        InvalidHostName
    }

    public class SingleNameSelectionStrategy : IIPSelectionStrategy
    {
        private IPAddress[] _ipArray;

        public SingleNameSelectionStrategy(string hostName)
        {
            if (!NetHelp.IsValidDnsName(hostName) && !NetHelp.IsValidIpAddress(hostName))
            {
                throw new InvalidAddressException("Invalid host name.", "hostName", InvalidityReason.InvalidHostName);
            }

            HostName = hostName;
        }

        public string HostName
        {
            get;
            private set;
        }

        private void FindIPs()
        {
            try
            {
                var hostAddresses = Dns.GetHostAddresses(HostName);
                var extv4 = hostAddresses.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip)).ToArray();
                if (extv4.Count() == 0)
                    extv4 = hostAddresses.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToArray();
                _ipArray = extv4.Count() > 0 ?
                    extv4 : hostAddresses;
            }
            catch (SocketException ex)
            {
                LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network, "Error resolving host '{0}': {1}", HostName, ex.Message);
                throw new InvalidOperationException(String.Format("Error resolving host '{0}' using DNS.", HostName));
            }
        }

        #region IIpSelectionStarategy Members

        public IEnumerable<IPAddress> GetIPs()
        {
            if (_ipArray == null)
            {
                FindIPs();
            }

            return _ipArray;
        }

        #endregion
    }

    public class SingleIPSelectionStrategy : IIPSelectionStrategy
    {
        public SingleIPSelectionStrategy(IPAddress ip)
        {
            IP = ip;
        }

        public IPAddress IP
        {
            get;
            private set;
        }

        public IEnumerable<IPAddress> GetIPs()
        {
            return new[] { IP };
        }
    }

    public class MaskIPSelectionStrategy : IIPSelectionStrategy
    {
        private List<IPAddress> _ipList;
        private IPAddress _from;
        private IPAddress _to;

        public IPAddress IP
        {
            get;
            private set;
        }

        public IPAddress Mask
        {
            get;
            private set;
        }

        public MaskIPSelectionStrategy(IPAddress ip, IPAddress mask)
        {
            IP = ip;
            Mask = mask;

            if (!IsSubnetMaskValid(Mask))
            {
                throw new InvalidAddressException("Invalid subnet mask.", "mask", InvalidityReason.InvalidSubnetMask);
            }

            byte[] ipBytes = IP.GetAddressBytes();
            byte[] maskBytes = Mask.GetAddressBytes();

            byte[] ipFromBytes = new byte[4];
            byte[] ipToBytes = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                ipFromBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                ipToBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
            }

            _from = new IPAddress(ipFromBytes);
            _to = new IPAddress(ipToBytes);

            ulong ipCount = RangeIPSelectionStrategy.GetIPCount(_from, _to);

            if (ipCount > (UInt64)UInt16.MaxValue + 1)
            {
                throw new InvalidAddressException("Too many IP addresses in the range.", "to", InvalidityReason.TooManyAddressesInRange);
            }
        }

        private void FindIPs()
        {
            _ipList = RangeIPSelectionStrategy.GetIPList(_from, _to);
        }

        public static bool IsSubnetMaskValid(IPAddress mask)
        {
            if (mask.IsIPv6LinkLocal || mask.IsIPv6Multicast || mask.IsIPv6SiteLocal || mask.IsIPv6Teredo)
                return false;

            uint next;
            uint last = IPAddressToUint(mask);
            for (int i = 0; i < 32; i++)
            {
                next = last << 1;
                if (next > last)
                {
                    return false;
                }
                last = next;
            }
            return true;
        }

        public static uint IPAddressToUint(IPAddress ip)
        {
            byte[] ipBytes = ip.GetAddressBytes();
            uint ipInt = 0;

            ipInt |= (uint)ipBytes[0] << 24;
            ipInt |= (uint)ipBytes[1] << 16;
            ipInt |= (uint)ipBytes[2] << 8;
            ipInt |= (uint)ipBytes[3];

            return ipInt;
        }

        #region IIpSelectionStarategy Members

        public IEnumerable<IPAddress> GetIPs()
        {
            if (_ipList == null)
            {
                FindIPs();
            }

            return _ipList;
        }

        #endregion
    }

    public class RangeIPSelectionStrategy : IIPSelectionStrategy
    {
        private List<IPAddress> _ipList;

        public IPAddress FromIP
        {
            get;
            private set;
        }

        public IPAddress ToIP
        {
            get;
            private set;
        }

        public RangeIPSelectionStrategy(IPAddress from, IPAddress to)
        {
            FromIP = from;
            ToIP = to;

            UInt64 ipCount = GetIPCount(FromIP, ToIP);
            if (ipCount > (UInt64)UInt16.MaxValue + 1)
            {
                throw new InvalidAddressException("Too many IP addresses in the range.", "to", InvalidityReason.TooManyAddressesInRange);
            }
        }

        public static List<IPAddress> GetIPList(IPAddress from, IPAddress to)
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
                retValue.Add(EnumerableIntToIPAddress(addr));
            }

            return retValue;
        }

        private static UInt32 IPAddressToEnumerableInt(IPAddress ip)
        {
            byte[] ipBytes = ip.GetAddressBytes();
            Array.Reverse(ipBytes);
            return BitConverter.ToUInt32(ipBytes, 0);
        }

        private static IPAddress EnumerableIntToIPAddress(UInt32 addr)
        {
            byte[] ipBytes = BitConverter.GetBytes(addr);
            Array.Reverse(ipBytes);
            return new IPAddress(ipBytes);
        }

        public static UInt64 GetIPCount(IPAddress from, IPAddress to)
        {
            byte[] fromBytes = from.GetAddressBytes();
            byte[] toBytes = to.GetAddressBytes();

            Array.Reverse(fromBytes);
            Array.Reverse(toBytes);

            uint fromAddress = BitConverter.ToUInt32(fromBytes, 0);
            uint toAddress = BitConverter.ToUInt32(toBytes, 0);

            if (fromAddress > toAddress)
            {
                return (UInt64)(fromAddress - toAddress) + 1;
            }
            return (UInt64)(toAddress - fromAddress) + 1;
        }

        #region IIpSelectionStarategy Members

        public IEnumerable<IPAddress> GetIPs()
        {
            if (_ipList == null)
            {
                _ipList = GetIPList(FromIP, ToIP);
            }

            return _ipList;
        }

        #endregion
    }
}
