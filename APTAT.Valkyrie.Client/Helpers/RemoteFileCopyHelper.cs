using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.Client.Helpers
{
    internal static class RemoteFileCopyHelper
    {
        public static string CopyFiles(List<ValkyrieFileInfo> badFiles, string login, string password)
        {
            var path = CreateTmpFolder();
            Parallel.ForEach(badFiles,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 5
                },
                file =>
                {
                    GetRemoteFile(file.Path, file.Sha1, login, password, file.Ip, path);
                });

            return path;
        }

        private static string CreateTmpFolder()
        {
            
            var usrTempFolder = Environment.GetEnvironmentVariable("TEMP");
            if (string.IsNullOrWhiteSpace(usrTempFolder))
            {
                usrTempFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            var valkyrieFolder = Path.Combine(usrTempFolder, "ValkyrieTmp");
            bool exists = Directory.Exists(valkyrieFolder);
            if (!exists)
            {
                Directory.CreateDirectory(valkyrieFolder);
            }
            else
            {
                DirectoryInfo downloadedMessageInfo = new DirectoryInfo(valkyrieFolder);

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
            if (fileName == null) throw new ArgumentNullException("fileName");

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe"
            };
            var host = "\\\\" + ipAddress; //  Items.First(i => i.TempItems.Any(ti => ti.BadFile.Sha1.Equals(badFile.Sha1))).IpAddress;
            var target = host + "\\" + Path.GetPathRoot(fileName).Replace(":\\", "$");
            string destination = Path.Combine(destPath, sha1);
            string source = target + fileName.Remove(0, 2);
            startInfo.Arguments = string.Format("/C net use \"{0}\" {1} /user:{2} & copy \"{3}\" {4}.tmp", target, password, login, source, destination);

            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

        }

    }
}
