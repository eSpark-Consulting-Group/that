using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ResourceCopyTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string pathToExe = "";
            if (args.Length != 2)
            {
                Console.WriteLine("Please provide 2 arguments. First source file name and second destination file name");
                return;
            }
            pathToExe = args[0];
            var destFile = args[1];
            var versionInfo = FileVersionInfo.GetVersionInfo(pathToExe);
            string version = versionInfo.ProductVersion;
            int major = versionInfo.FileMajorPart;
            string template = BuildRes(version.Replace(".", ","), version, versionInfo, destFile);
            File.WriteAllText("version.rc", template, Encoding.Unicode);
        }

        private static string BuildRes(string commaSeparatedValues, string dotSeparatedValues, FileVersionInfo version, string destFile)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Empty);
            sb.AppendLine("1 VERSIONINFO");
            sb.AppendLine("FILEVERSION " + commaSeparatedValues);
            sb.AppendLine("PRODUCTVERSION " + commaSeparatedValues);
            sb.AppendLine("FILEOS 0x40004");
            sb.AppendLine("FILETYPE 0x1");
            sb.AppendLine("{");
            sb.AppendLine("BLOCK \"StringFileInfo\"");
            sb.AppendLine("{");
            sb.AppendLine("\tBLOCK \"040904B0\"");
            sb.AppendLine("\t{");
            sb.AppendLine(string.Format("\t\tVALUE \"CompanyName\", \"{0}\"", version.CompanyName));
            sb.AppendLine(string.Format("\t\tVALUE \"FileDescription\", \"{0}\"", version.FileDescription));
            sb.AppendLine(string.Format("\t\tVALUE \"FileVersion\", \"{0}\"", dotSeparatedValues));
            sb.AppendLine(string.Format("\t\tVALUE \"InternalName\", \"{0}\"", destFile));
            sb.AppendLine(string.Format("\t\tVALUE \"LegalCopyright\", \"{0} {1}\"", version.LegalCopyright, version.CompanyName));
            sb.AppendLine(string.Format("\t\tVALUE \"OriginalFilename\", \"{0}\"", destFile));
            sb.AppendLine(string.Format("\t\tVALUE \"ProductName\", \"{0}\"", version.ProductName));
            sb.AppendLine(string.Format("\t\tVALUE \"ProductVersion\", \"{0}\"", dotSeparatedValues));
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("BLOCK \"VarFileInfo\"");
            sb.AppendLine("{");
            sb.AppendLine("\tVALUE \"Translation\", 0x0409 0x04B0");
            sb.AppendLine("}");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
