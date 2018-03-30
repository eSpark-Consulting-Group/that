using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APTAT.VersionChanger
{
    class Program
    {
        private static readonly string[] ASSEMBLY_INFO_PATTERNS = { "AssemblyInfo.cs", "CommonAssemblyInfo.cs" };
        private static readonly string[] ASSEMBLY_INFO_VERSION_PATTERNS = { "[assembly: AssemblyVersion", "[assembly: AssemblyFileVersion" };

        /// <summary>
		/// A regex that matches strings like 'AssemblyVersion("1.0.0.1")'
		/// </summary>
		private static readonly Regex AssemblyVersionRegex =
            new Regex("AssemblyVersion(?:Attribute)?\\(\\s*?\"(?<version>(?<major>[0-9]+)\\.(?<minor>[0-9]+)\\.(?<build>[0-9]+)\\.(?<revision>[0-9]+))\"\\s*?\\)");

        private static readonly Regex FileVersionRegex =
            new Regex("AssemblyFileVersion(?:Attribute)?\\(\\s*?\"(?<version>(?<major>[0-9]+)\\.(?<minor>[0-9]+)\\.(?<build>[0-9]+)\\.(?<revision>[0-9]+))\"\\s*?\\)");

        private static readonly string AssemblyVersionReplaceFormat = "AssemblyVersion(\"{0}\")";
        private static readonly string AssemblyFileVersionReplaceFormat = "AssemblyFileVersion(\"{0}\")";

        static void Main(string[] args)
        {
#if DEBUG
            //args = new string[] { @"..\..\..\..\..\APTAT", @"1.0.0.4" };
#endif

            Console.WriteLine(Environment.CurrentDirectory);

            if (args.Length != 2)
            {
                Console.WriteLine("Specify folder path and version");
                return;
            }

            var folderPath = Path.GetFullPath(args[0]);
            var version = args[1];

            Console.WriteLine($"Full path is: {folderPath}");
            Console.WriteLine($"New version is: {version}");

            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                Console.WriteLine($"Specify folder path or {folderPath} does not exist");
                return;
            }

            if (version.Split('.').Length != 4)
            {
                Console.WriteLine($"version {version} is not valid");
                return;
            }

            var files = GetAllFilesToChangeVersion(folderPath);
            Console.WriteLine(string.Join(Environment.NewLine, files));

            foreach (var file in files)
            {
                ChangeVersion(file, version);
            }
        }

        private static void ChangeVersion(string assembly, string newVersion)
        {
            var content = File.ReadAllText(assembly);
            var assemlyMatch = AssemblyVersionRegex.Match(content);
            var fileMatch = FileVersionRegex.Match(content);

            if (assemlyMatch != null)
            {
                var assemblyReplace = string.Format(AssemblyVersionReplaceFormat, newVersion);
                content = AssemblyVersionRegex.Replace(content, assemblyReplace, 1);
            }

            if (fileMatch != null)
            {
                var fileReplace = string.Format(AssemblyFileVersionReplaceFormat, newVersion);
                content = FileVersionRegex.Replace(content, fileReplace, 1);
            }

            File.WriteAllText(assembly, content);
        }

        private static IEnumerable<string> GetAllFilesToChangeVersion(string folderPath)
        {
            var result = new List<string>();
            GetAllFilesToChangeVersionRecursivly(folderPath, result);
            return result;
        }

        private static void GetAllFilesToChangeVersionRecursivly(string folderPath, List<string> files)
        {
            if (Directory.Exists(folderPath))
            {
                var dirInfo = new DirectoryInfo(folderPath);
                var directories = dirInfo.GetDirectories();

                files.AddRange(dirInfo.GetFiles().Where(f => ASSEMBLY_INFO_PATTERNS.Contains(Path.GetFileName(f.FullName))).Select(f => f.FullName));

                foreach (var dir in directories.Select(d => d.FullName))
                {
                    GetAllFilesToChangeVersionRecursivly(dir, files);
                }
            }
        }
    }
}
