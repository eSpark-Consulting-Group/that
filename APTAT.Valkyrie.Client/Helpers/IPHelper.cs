using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace APTAT.Valkyrie.Client.Helpers
{
    public class NetAddressHelper
    {
    
        // ReSharper disable once InconsistentNaming
        public static string GetMacWithIPAddress()
        {
            const int minMacAddrLength = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            string ipAddress = String.Empty;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed &&!string.IsNullOrEmpty(tempMac) &&
                    tempMac.Length >= minMacAddrLength)
                {

                    var ips = nic.GetIPProperties().UnicastAddresses;

                    foreach (UnicastIPAddressInformation ip in ips)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddress= ip.Address.ToString();
                        }
                    }
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }

            return string.Format("{0}_{1}", macAddress, ipAddress);
        }
    }
}