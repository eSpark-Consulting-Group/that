using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace APTAT.Core
{
    public static class MachineKey
    {
        static MachineKey()
        {
            GetSerials();

            using (var md5 = MD5.Create())
            {
                byte[] data = Encoding.UTF8.GetBytes(String.Join(" ## ", new[] { _softwareId, _hardwareId }));
                _cryptedMachineKey = md5.ComputeHash(data);
            }
        }

        private static string _softwareId;
        private static string _hardwareId;
        private static readonly byte[] _cryptedMachineKey;

        public static string SoftwareId
        {
            get { return _softwareId; }
        }

        public static string HardwareId
        {
            get { return _hardwareId; }
        }

        public static void GetSerials()
        {
            using (var query = new ManagementObjectSearcher("Select * From Win32_OperatingSystem"))
            using (var queryCollection = query.Get())
            {
                var serial = queryCollection
                    .OfType<ManagementObject>()
                    .Select(mo => mo["SerialNumber"])
                    .FirstOrDefault(sn => sn != null);
                _softwareId = "" + serial;
            }
            using (var query = new ManagementObjectSearcher("SELECT * From Win32_BaseBoard"))
            using (var queryCollection = query.Get())
            {
                var serial = queryCollection
                    .OfType<ManagementObject>()
                    .Select(mo => mo["SerialNumber"])
                    .FirstOrDefault(pi => pi != null);
                _hardwareId = "" + serial;
            }
        }

        public static string CryptedMachineKey
        {
            get { return _cryptedMachineKey.Aggregate(new StringBuilder(), (sb, b) => sb.Append(b.ToString("X2")), sb => sb.ToString()); }
        }

        public static new string ToString()
        {
            return String.Format("SoftwareID: {0}, HardwareID: {1}", _softwareId, _hardwareId);
        }
    }
}