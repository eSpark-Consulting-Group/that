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
        public RegistryKey OpenSubKey(String key)
        {
           return Registry.LocalMachine.OpenSubKey(key);            
        }
    }
}
