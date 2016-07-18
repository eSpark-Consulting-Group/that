using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace APTAT.Application.Models
{
    public class IpAddressValidator : AbstractValidator<IpAddressWrapper>
    {
        // Must be in sync with the Esm.Server.Facade.Descent.Commands.GetTargetsSummaryCommand.MaxDeploymentTargets constant!
        private const int MaxTargetsNetworkBits = 10;
        private const int MaxTargetsCount = 1024;

        public IpAddressValidator()
        {
            RuleFor(o => o.Text)
                .Must((o, v) =>
                {
                    o.IsMask = IsValidMask(o.Text);
                    o.IsRange = IsValidRange(o.Text);
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
                    if (IsValidIp(o.Text, out value))
                    {
                        o.FormattedText = value.ToString();
                        return true;
                    }
                    else
                        return o.IsDnsName = IsValidDnsName(o.Text);
                })
                .WithMessage("Please enter valid IPv4 address subnet or range");

            RuleFor(o => o.Text)
                .Must((o, v) =>
                {
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
                })
                .WithMessage("Address range is too large");
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
            // Mask is specified by length
            if (Byte.TryParse(splits[1], out length))
                return length <= 32;
            // Mask is specified by IP
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

        // For a host name to be valid it must contain at least one letter, to avoid mistyped IP addressed to be validated as host names
        private static Regex s_dnsNamePattern1 = new Regex(@"[A-Za-z_]+");
        private static Regex s_dnsNamePattern2 = new Regex(@"^(?=.{1,255}$)[0-9A-Za-z_](?:(?:[0-9A-Za-z_]|\b-){0,61}[0-9A-Za-z_])?(?:\.[0-9A-Za-z_](?:(?:[0-9A-Za-z_]|\b-){0,61}[0-9A-Za-z_])?)*$");

        public static bool IsValidDnsName(string addr)
        {
            return !String.IsNullOrWhiteSpace(addr) && addr.Length < 256 && s_dnsNamePattern1.IsMatch(addr) && s_dnsNamePattern2.IsMatch(addr);
        }

        public static bool IsValidIpWithPort(string endpoint)
        {
            if (String.IsNullOrWhiteSpace(endpoint))
                return false;
            var parts = endpoint.Split(':');
            if (parts.Length > 2 || parts.Length < 1)
                return false;
            IPAddress ipValue;
            if (!IsValidIp(parts[0], out ipValue))
                return false;
            if (parts.Length > 1)
            {
                ushort portValue;
                if (!UInt16.TryParse(parts[1], out portValue))
                    return false;
            }
            return true;
        }

        public static bool IsValidDnsNameWithPort(string endpoint)
        {
            if (String.IsNullOrWhiteSpace(endpoint))
                return false;
            var parts = endpoint.Split(':');
            if (parts.Length > 2 || parts.Length < 1)
                return false;
            if (!IsValidDnsName(parts[0]))
                return false;
            if (parts.Length > 1)
            {
                ushort portValue;
                if (!UInt16.TryParse(parts[1], out portValue))
                    return false;
            }
            return true;
        }

        public static bool IsValidPort(int port)
        {
            return port > 0 && port <= UInt16.MaxValue;
        }

        public static bool IsValidWebAddress(string endpoint, IEnumerable<string> shemas = null)
        {
            if (String.IsNullOrWhiteSpace(endpoint))
                return false;

            UriBuilder uri;

            try
            {
                uri = new UriBuilder(new Uri(endpoint));
                Uri test = uri.Uri;
            }
            catch
            {
                return false;
            }

            string shema = uri.Scheme;
            string host = uri.Host;
            int port = uri.Port;

            if (shemas != null)
            {
                bool isValidShema = false;
                foreach (var item in shemas)
                {
                    if (shema == item)
                    {
                        isValidShema = true;
                        break;
                    }
                }

                if (!isValidShema)
                    return false;
            }

            bool isValidDns = IsValidDnsName(host);
            bool isValidIp = IsValidIpWithPort(host);
            bool isValidPort = IsValidPort(port);
            return (isValidDns || isValidIp) && isValidPort;
        }
    }

    public class IpAddressWrapper : DomainObject, IComparable<IpAddressWrapper>, IComparable
    {
        public static readonly IValidator Validator = new IpAddressValidator();

        public bool IsMask { get; set; }

        public bool IsRange { get; set; }

        public bool IsDnsName { get; set; }

        private string _text;
        private string _formattedText;

        public String Text
        {
            get { return _text; }
            set
            {
                _text = value.Replace(" ", "");
                RaisePropertyChanged("Text");
                RaisePropertyChanged("FormattedText");

         
                ValidatePropertyThrow("Text", Validator);
            }
        }

        public string FormattedText
        {
            get { return _formattedText ?? _text; }
            set
            {
                _formattedText = value;
                RaisePropertyChanged("FormattedText");
            }
        }

        public void SplitMask(out IPAddress network, out IPAddress mask, out int maskLength)
        {
            var splits = Text.Split('/');
            network = IPAddress.Parse(splits[0]);
            byte length;
            if (Byte.TryParse(splits[1], out length))
            {
                var zeroesLen = 32 - length;
                var maskBits = (UInt32.MaxValue >> zeroesLen) << zeroesLen;
                mask = GetIPFromUInt32(maskBits);
                maskLength = length;
            }
            else
            {
                mask = IPAddress.Parse(splits[1]);
                maskLength = 32;
                var maskBits = GetUInt32FromIP(mask);
                while ((maskBits & 1) == 0 && maskLength > 0)
                {
                    maskBits >>= 1;
                    maskLength--;
                }
            }
        }

        public void SplitRange(out IPAddress result1, out IPAddress result2)
        {
            var splits = Text.Split('-');
            result1 = IPAddress.Parse(splits[0]);
            result2 = IPAddress.Parse(splits[1]);
        }

        public static IPAddress GetIPFromUInt32(uint value)
        {
            return new IPAddress((uint)(IPAddress.HostToNetworkOrder(value) >> 32));
        }

        public static uint GetUInt32FromIP(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            uint value = BitConverter.ToUInt32(bytes, 0);
            return (uint)(IPAddress.NetworkToHostOrder(value) >> 32);
        }

        public EndpointAddressWrapper GetEndpointAddress()
        {
            if (IsMask)
            {
                int maskLength;
                IPAddress network, mask;
                SplitMask(out network, out mask, out maskLength);
                return new SubnetAddressWrapper(new SubnetAddress { IPAddress = network.ToString(), SubnetMask = mask.ToString() });
            }
            if (IsRange)
            {
                IPAddress start, end;
                SplitRange(out start, out end);
                return new IPRangeAddressWrapper(new IPRangeAddress { StartAddress = start.ToString(), EndAddress = end.ToString() });
            }
            if (IsDnsName)
            {
                return new DnsNameAddressWrapper(new DnsNameAddress { DnsName = Text });
            }
            else
            {
                return new SingleIPAddressWrapper { IPAddress = FormattedText };
            }
        }

        public void ClearValidationError()
        {
            RaisePropertyChanged("Text");
        }

        public int CompareTo(IpAddressWrapper other)
        {
            return other != null ? FormattedText.CompareTo(other.FormattedText) : 1;
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo(obj as IpAddressWrapper);
        }

        public override string ToString()
        {
            return FormattedText;
        }
    }

    public class DomainObject<T> : DomainObject
    {
        public virtual T BaseEntity
        {
            get;
            set;
        }
    }

    public abstract class EndpointAddressWrapper : DomainObject<BaseEndpointAddress>
    {
        protected BaseEndpointAddress _address;

        public override BaseEndpointAddress BaseEntity
        {
            get { return _address; }
            set { _address = value; }
        }
    }

    public abstract class EndpointAddressWrapper<T> : EndpointAddressWrapper where T : BaseEndpointAddress
    {
        public T EndpointAddress
        {
            get { return (T)_address; }
        }
    }

    public class DnsNameAddressWrapper : EndpointAddressWrapper<DnsNameAddress>
    {
        public DnsNameAddressWrapper(DnsNameAddress dnsNameAddress)
        {
            _address = dnsNameAddress;
        }

        public string DnsName
        {
            get { return EndpointAddress.DnsName; }
            set
            {
                if (EndpointAddress.DnsName == value)
                    return;
                EndpointAddress.DnsName = value;
                RaisePropertyChanged("DnsName");
            }
        }
    }

    public class IPRangeAddressWrapper : EndpointAddressWrapper<IPRangeAddress>
    {
        public IPRangeAddressWrapper(IPRangeAddress ipRangeAddress)
        {
            _address = ipRangeAddress;
        }

        public string StartAddress
        {
            get { return EndpointAddress.StartAddress; }
            set
            {
                if (EndpointAddress.StartAddress == value)
                    return;
                EndpointAddress.StartAddress = value;
                RaisePropertyChanged("StartAddress");
            }
        }

        public string EndAddress
        {
            get { return EndpointAddress.EndAddress; }
            set
            {
                if (EndpointAddress.EndAddress == value)
                    return;
                EndpointAddress.EndAddress = value;
                RaisePropertyChanged("EndAddress");
            }
        }
    }

    public class SubnetAddressWrapper : EndpointAddressWrapper<SubnetAddress>
    {
        public SubnetAddressWrapper(SubnetAddress subnetAddress)
        {
            _address = subnetAddress;
        }

        public string IPAddress
        {
            get { return EndpointAddress.IPAddress; }
            set
            {
                if (EndpointAddress.IPAddress == value)
                    return;
                EndpointAddress.IPAddress = value;
                RaisePropertyChanged("IPAddress");
            }
        }

        public string SubnetMask
        {
            get { return EndpointAddress.SubnetMask; }
            set
            {
                if (EndpointAddress.SubnetMask == value)
                    return;
                EndpointAddress.SubnetMask = value;
                RaisePropertyChanged("SubnetMask");
            }
        }
    }

    public class SingleIPAddressWrapper : EndpointAddressWrapper<SingleIPAddress>
    {
        public SingleIPAddressWrapper(SingleIPAddress singleIpAddress)
        {
            _address = singleIpAddress;
        }

        public SingleIPAddressWrapper()
        {
            _address = new SingleIPAddress();
        }

        public string IPAddress
        {
            get { return EndpointAddress.IPAddress; }
            set
            {
                if (EndpointAddress.IPAddress == value)
                    return;
                EndpointAddress.IPAddress = value;
                RaisePropertyChanged("IPAddress");
            }
        }
    }
}


