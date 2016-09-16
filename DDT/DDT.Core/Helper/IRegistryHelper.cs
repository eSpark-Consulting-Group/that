using Microsoft.Win32;

namespace DDT.Core.Helper
{
    public interface IRegistryHelper
    {
        //RegistryKey OpenSubKey(string key);
        bool IsSubKeyExists(string key);
        bool IsValueExists(string key, string value);
    }
}