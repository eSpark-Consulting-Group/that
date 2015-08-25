using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace APTAT.Valkyrie.Client.Helpers
{
    internal static class RemoteFileCopyHelper
    {
        public static string CopyFiles(List<ValkyrieFileInfo> badFiles, string login, string password)
        {
            var path = CreateTmpFolder();
            foreach (var file in badFiles)
            {
                GetRemoteFile(file.Path, file.Sha1, login, password, file.Ip, path);
            }
            return path;
        }

        private static string CreateTmpFolder()
        {
            var DefaultLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var usrTempFolder = System.Environment.GetEnvironmentVariable("TEMP");
            var valkyrieFolder = Path.Combine(usrTempFolder, "ValkyrieTmp");
            bool exists = System.IO.Directory.Exists(valkyrieFolder);
            if (!exists)
            {
                System.IO.Directory.CreateDirectory(valkyrieFolder);
            }
            else
            {
                System.IO.DirectoryInfo downloadedMessageInfo = new DirectoryInfo(valkyrieFolder);

                foreach (FileInfo file in downloadedMessageInfo.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in downloadedMessageInfo.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            return valkyrieFolder;
        }

        private static void GetRemoteFile(string fileName, string sha1, string login, string password, string ipAddress, string destPath)
        {

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            var host = "\\\\" + ipAddress; //  Items.First(i => i.TempItems.Any(ti => ti.BadFile.Sha1.Equals(badFile.Sha1))).IpAddress;
            var target = host + "\\" + System.IO.Path.GetPathRoot(fileName).Replace(":\\", "$");
            string destination = Path.Combine(destPath, sha1);
            string source = target + fileName.Remove(0, 2);
            startInfo.Arguments = string.Format("/C net use \"{0}\" {1} /user:{2} & copy \"{3}\" {4}.tmp", target, password, login, source, destination);

            process.StartInfo = startInfo;
            process.Start();
        }

    }
}
