
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace DDT.Application.Models
{
    public class IpAddressValidator
    {
        private const int MaxTargetsNetworkBits = 10;
        private const int MaxTargetsCount = 1024;

        public bool Validate1(IpAddressWrapper o, out string s)
        {
            s = "Please enter valid IPv4 address subnet or range";
            o.IsMask = IsValidMask(o.Value);
            o.IsRange = IsValidRange(o.Value);
            o.IsDnsName = false;

            if (o.IsMask)
            {
                int maskLength;
                IPAddress network, mask;
                o.SplitMask(out network, out mask, out maskLength);
                o.FormattedText = network + "/" + mask;
                return true;
            }
            if (o.IsRange)
            {
                IPAddress begin, end;
                o.SplitRange(out begin, out end);
                var beginBits = IpAddressWrapper.GetUInt32FromIP(begin);
                var endBits = IpAddressWrapper.GetUInt32FromIP(end);
                if (beginBits <= endBits)
                {
                    o.FormattedText = begin + "-" + end;
                    return true;
                }
                else
                    return false;
            }
            IPAddress value;
            if (IsValidIp(o.Value, out value))
            {
                o.FormattedText = value.ToString();
                return true;
            }
            else
                return o.IsDnsName = IsValidDnsName(o.Value);
        }

        public bool Validate2(IpAddressWrapper o, out string s)
        {
            s = "Network Address range is too large";
            if (o.IsMask)
            {
                int maskLength;
                IPAddress network, mask;
                o.SplitMask(out network, out mask, out maskLength);
                return maskLength > MaxTargetsNetworkBits;
            }
            if (o.IsRange)
            {
                IPAddress begin, end;
                o.SplitRange(out begin, out end);
                uint beginBits = IpAddressWrapper.GetUInt32FromIP(begin);
                uint endBits = IpAddressWrapper.GetUInt32FromIP(end);
                return endBits - beginBits < MaxTargetsCount;
            }
            return true;
                
        }

        private static readonly Regex IpV4Pattern = new Regex(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");

        public static bool IsValidIp(string addr, out IPAddress value)
        {
            value = null;
            if (String.IsNullOrWhiteSpace(addr))
            {
                return false;
            }

            if (IpV4Pattern.IsMatch(addr) && IPAddress.TryParse(addr, out value)
                && IsInValidRangeIp(value))
            {
                return true;
            }

            return false;
        }

        public static bool IsValidMask(string addr)
        {
            if (String.IsNullOrWhiteSpace(addr))
                return false;
            var splits = addr.Split('/');
            if (splits.Length != 2)
                return false;
            IPAddress ip;
            if (!IsValidIp(splits[0], out ip))
                return false;
            byte length;
            if (Byte.TryParse(splits[1], out length))
                return length <= 32;
            if (!IsValidIp(splits[1], out ip))
                return false;
            var mask = IpAddressWrapper.GetUInt32FromIP(ip);
            while ((mask & 1) == 0)
                mask = (mask >> 1) | 0x80000000;
            return mask == UInt32.MaxValue;
        }

        public static bool IsValidRange(string addr)
        {
            if (String.IsNullOrWhiteSpace(addr))
                return false;
            var splits = addr.Split('-');
            if (splits.Length == 2)
            {
                IPAddress value;
                return IsValidIp(splits[0], out value) &&
                       IsValidIp(splits[1], out value);
            }
            else
                return false;
        }

        public static bool IsInValidRangeIp(IPAddress addr)
        {
            if (addr.GetAddressBytes()[0] == 0)
                return false;
            return true;
        }

        private static Regex s_dnsNamePattern1 = new Regex(@"[A-Za-z_]+");
        private static Regex s_dnsNamePattern2 = new Regex(@"^(?=.{1,255}$)[0-9A-Za-z_](?:(?:[0-9A-Za-z_]|\b-){0,61}[0-9A-Za-z_])?(?:\.[0-9A-Za-z_](?:(?:[0-9A-Za-z_]|\b-){0,61}[0-9A-Za-z_])?)*$");

        public static bool IsValidDnsName(string addr)
        {
            return !String.IsNullOrWhiteSpace(addr) && addr.Length < 256 && s_dnsNamePattern1.IsMatch(addr) && s_dnsNamePattern2.IsMatch(addr);
        }
    }
}