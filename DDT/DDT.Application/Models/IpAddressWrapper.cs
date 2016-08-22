using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using System;
using System.Net;

namespace DDT.Application.Models
{
    public class IpAddressWrapper : SelectableItemWrapper<string>, IComparable<IpAddressWrapper>, IComparable
    {
        public bool IsMask { get; set; }

        public bool IsRange { get; set; }

        public bool IsDnsName { get; set; }
        
        private string _formattedText;

        public override String Value
        {
            get { return base.Value; }
            set
            {
                base.Value = value.Replace(" ", "");
                RaisePropertyChanged(nameof(FormattedText));
            }
        }

        public string FormattedText
        {
            get { return _formattedText ?? Value; }
            set
            {
                _formattedText = value;
                RaisePropertyChanged(nameof(FormattedText));
            }
        }

        public void SplitMask(out IPAddress network, out IPAddress mask, out int maskLength)
        {
            var splits = Value.Split('/');
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
            var splits = Value.Split('-');
            result1 = IPAddress.Parse(splits[0]);
            result2 = IPAddress.Parse(splits[1]);
        }

        public static IPAddress GetIPFromUInt32(uint value)
        {
            return new IPAddress((uint) (IPAddress.HostToNetworkOrder(value) >> 32));
        }

        public static uint GetUInt32FromIP(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            uint value = BitConverter.ToUInt32(bytes, 0);
            return (uint) (IPAddress.NetworkToHostOrder(value) >> 32);
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
                return new IpRangeAddressWrapper(new IPRangeAddress { StartAddress = start.ToString(), EndAddress = end.ToString() });
            }
            if (IsDnsName)
            {
                return new DnsNameAddressWrapper(new DnsNameAddress { DnsName = Value });
            }
            else
            {
                return new SingleIPAddressWrapper { IPAddress = FormattedText };
            }
        }

        public int CompareTo(IpAddressWrapper other)
        {
            return other != null ? FormattedText.CompareTo(other.FormattedText) : 1;
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo(obj as IpAddressWrapper);
        }
    }
}