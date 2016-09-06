using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;

namespace DDT.Core.Helper
{
    public sealed class CheckNmapInstallation
    {        
        public bool Check(out string message)
        {
            string errorMessage = String.Empty;
            if (!IsVCRedist2013Installed())
            {                
                message = "Visual C++ Redistributable 2013 is not installed";
                return false;                
            }

            if (!IsApplicationWinPcapInstalled())
            {
                message = "WinPcap is not installed";
                return false;
            }

            if (!IsApplicationNmapInstalled())
            {
                message = "Nmap is not installed";
                return false;
            }

            message = String.Empty;
            return true;          
        }

        private static bool IsApplicationNmapInstalled()
        {
            var result = true;

            if (!IsProcessExist(@"nmap.exe"))
            {
                if (!IsProcessExist(@"C:\Program Files\Nmap\nmap.exe"))
                {
                    if (!IsProcessExist(@"C:\Program Files (x86)\Nmap\nmap.exe"))
                    {
                        result = false;
                    }
                }
            }
            return result;
        }
        private static bool IsApplicationWinPcapInstalled()
        {            
            bool result = true;

            var key86 =
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\WinPcapInst");

          
            var key64 =
                Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\WinPcapInst");
          

            if (key86 != null || key64 != null)
            {
                if (!File.Exists(@"C:\Windows\System32\drivers\NPF.sys"))
                {                
                    result = false;
                }
            }
            else
            {
                result = false;
            }            
            return result;
        }

        private static bool IsVCRedist2013Installed()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                var keyX86On64 = @"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\12.0\VC\Runtimes\x86";
                if (GetValueOfInstalled(keyX86On64))
                {
                    return true;
                }
                else
                {
                    var keyX64 = @"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\12.0\VC\Runtimes\x64";
                    return GetValueOfInstalled(keyX64);
                }
            }
            else
            {
                var keyX86 = @"SOFTWARE\Microsoft\VisualStudio\12.0\VC\Runtimes\x86";
                return GetValueOfInstalled(keyX86);
            }
        }
        
        private static bool IsProcessExist(string path)
        {
            var result = true;
            Process processNmap = null;

            try
            {
                processNmap = new Process
                {
                    StartInfo =
                    {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        FileName = path,
                        Arguments = "-sP"
                    }
                };

                processNmap.Start();
            }
            catch
            {
                
                result = false;
            }
            finally
            {
                if (processNmap != null && result)
                {
                    processNmap.Close();
                }
            }            
            
            return result;
        }

        private static bool GetValueOfInstalled(string key)
        {
            var subKey = Registry.LocalMachine.OpenSubKey(key);
            if (subKey == null)
                return false;
            var isInstalled = subKey.GetValue("Installed") as Nullable<int>;
            return isInstalled == 1;
        }
    }
}
