using System;
using System.Linq;

namespace APTAT.DeploymentCore.Directory
{
    public class DevicesSourceDescription : IDevicesSourceDescription
    {
        public ETargetsDiscovererType Type { get; set; }
        public string Description { get; set; }

        public static string GetNetworkDescription(string[] ipAddressesStringArray)
        {
            if (ipAddressesStringArray == null)
                throw new InvalidOperationException("Could not get Network Description");

            int maxElementsInResultString = 3;

            var resultAddrAray = ipAddressesStringArray.Take(maxElementsInResultString).ToArray();
            string resultAddrString = string.Join("; ", resultAddrAray);

            if (ipAddressesStringArray.Length > maxElementsInResultString)
                resultAddrString += " ...";

            return resultAddrString;
        }
    }

    public enum ETargetsDiscovererType
    {
        ActiveDirectory,
        NetworkAddresses,
        Workgroup
    }
}