using Microsoft.Win32;

namespace DDT.Core.Helper
{
    public interface IRegistryHelper
    {
        RegistryKey OpenSubKey(string key);
    }
}