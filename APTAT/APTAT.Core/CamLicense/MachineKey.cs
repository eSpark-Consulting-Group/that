using APTAT.Core.Descent.Wmi;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace APTAT.Core.CamLicense
{
    public static class MachineKey
    {
        static string REG_KEY_COMODO = @"SOFTWARE\COMODO";
        static string REG_VALUE_GLOBAL_MACHINE_ID = "GlobalMachineId";
        static MachineKey()
        {
            GlobalMachineKey = GenerateGlobalMachineId();
        }

        static string GenerateGlobalMachineId()
        {
            string machineIdStr = GetHddDataForHashingViaWMI();
            if (String.IsNullOrEmpty(machineIdStr))
            {
                machineIdStr = MachineKeyHelper.GetSerialNumberUsingStorageQuery(0);
            }
            if (!String.IsNullOrEmpty(machineIdStr))
            {
                SHA1 sha = new SHA1CryptoServiceProvider();
                // This is one implementation of the abstract class SHA1.
                byte[] bytes = Encoding.UTF8.GetBytes(machineIdStr);
                var result = sha.ComputeHash(bytes);
                return BitConverter.ToString(result).Replace("-", string.Empty);
            }
            else
            {
                return GetGeneratedFromGuid();
            }
        }
        static string GetGeneratedFromGuid()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(REG_KEY_COMODO, true))
            {
                if (key == null)
                {
                    using (var newKey = Registry.CurrentUser.CreateSubKey(REG_KEY_COMODO))
                    {
                        return CreateNewGlobalMachineId(newKey);
                    }
                }
                else
                {
                    var id = key.GetValue(REG_VALUE_GLOBAL_MACHINE_ID);
                    if (id == null)
                    {
                        return CreateNewGlobalMachineId(key);
                    }
                    else
                    {
                        return id.ToString();
                    }
                }

            }
        }

        static string CreateNewGlobalMachineId(RegistryKey key)
        {
            var guid = Guid.NewGuid();
            SHA1 sha = new SHA1CryptoServiceProvider();
            // This is one implementation of the abstract class SHA1.
            byte[] bytes = Encoding.UTF8.GetBytes(guid.ToString().ToUpper());
            var result = sha.ComputeHash(bytes);
            var generatedValue = BitConverter.ToString(result).Replace("-", string.Empty);
            if (key != null)
            {
                key.SetValue(REG_VALUE_GLOBAL_MACHINE_ID, generatedValue, RegistryValueKind.String);
            }
            return generatedValue;
        }

        static string GetHddDataForHashingViaWMI()
        {
            try
            {
                var path = "\\\\127.0.0.1\\root\\CIMV2";
                List<HddInfo> hdds = new List<HddInfo>();
                ConnectionOptions co;
                ManagementScope ms = null;
                ObjectQuery objectQuery = new SelectQuery("Win32_DiskDrive");
                co = WmiHelper.GetConnectionOptionsForWmi(null, null);
                ms = new ManagementScope(path, co);
                ms.Connect();
                ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(ms, objectQuery);
                managementObjectSearcher.Options.Timeout = TimeSpan.FromMinutes(2);
                foreach (var mo in managementObjectSearcher.Get())
                {
                    var deviceId = WmiHelper.GetProperty(mo, "DeviceId", "");
                    var model = WmiHelper.GetProperty(mo, "Model", "");
                    var signature = WmiHelper.GetProperty(mo, "Signature", "");
                    var totalTracks = WmiHelper.GetProperty(mo, "TotalTracks", "");
                    var serialNumber = WmiHelper.GetProperty(mo, "SerialNumber", "");
                    hdds.Add(new HddInfo(model, deviceId, serialNumber, signature, totalTracks));
                }
                var strForHashing = String.Empty;
                foreach (var hdd in hdds.OrderBy(h => h.DeviceId))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(hdd.Model);
                    sb.Append(hdd.Signature);
                    sb.Append(hdd.TotalTracks);
                    sb.Append(hdd.SerialNumber);
                    sb.Replace(" ", "");
                    strForHashing = sb.ToString();
                    break;
                }
                return strForHashing;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public static string GlobalMachineKey
        {
            get;
        }
    }
}