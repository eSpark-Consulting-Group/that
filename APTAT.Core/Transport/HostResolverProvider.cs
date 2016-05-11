using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace APTAT.Core.Transport
{

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
