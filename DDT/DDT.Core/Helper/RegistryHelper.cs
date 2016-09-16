using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDT.Core.Helper
{
    public class RegistryHelper : IRegistryHelper
    {
        public bool IsSubKeyExists(string key)
        {
            var subKey = Registry.LocalMachine.OpenSubKey(key);
            return subKey != null;
        }

        public bool IsValueExists(string key, string valueName)
        {
            var subKey = Registry.LocalMachine.OpenSubKey(key);
            if (subKey == null)
                return false;
            var isInstalled = subKey.GetValue(valueName) as bool?;
            return isInstalled == true;
        }
    }
}
