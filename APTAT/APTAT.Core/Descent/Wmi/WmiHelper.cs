using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Core.Descent.Wmi
{
    public static class WmiHelper
    {
        public static ConnectionOptions GetConnectionOptionsForWmi(string login, string password)
        {
            ConnectionOptions connectionOptions = new ConnectionOptions();
            connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
            connectionOptions.EnablePrivileges = true;
            connectionOptions.Authentication = AuthenticationLevel.PacketPrivacy;
            if (login != "" && password != "")
            {
                connectionOptions.Username = login;
                connectionOptions.Password = password;
            }
            return connectionOptions;
        }
        public static string GetProperty(ManagementBaseObject mo, string propertyName, string defaultVal)
        {
            string str = "";
            try
            {
                if (mo[propertyName] is ushort[])
                {
                    ushort[] numArray = mo[propertyName] as ushort[];
                    if (numArray.Length > 0)
                    {
                        str = string.Concat(numArray[0]);
                        for (int index = 1; index < numArray.Length; ++index)
                            str = str + (object)", " + (string)(object)numArray[index];
                    }
                }
                else
                    str = !(mo[propertyName] is string[]) ? mo[propertyName].ToString() : string.Join(", ", mo[propertyName] as string[]);
            }
            catch
            {
                str = defaultVal;
            }
            return str.Trim();
        }
    }
}
