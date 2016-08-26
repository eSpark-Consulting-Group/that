using System;
namespace DDT.Core.Descent
{
    public enum InstallPackageType
    {
        Msi, Exe
    }

    public enum TargetSystemType
    {
        x86, x64
    }     
    public class InstallPackageInfo
    {
        private InstallPackageInfo()
        {

        }
        public static InstallPackageInfo CreateMsiPackageInfo(string path, string mstPath, string parameters, TargetSystemType systemType, bool impersonate = false)
        {
            var package = new InstallPackageInfo();
            package.PackageType = InstallPackageType.Msi;
            package.Parameters = parameters;
            package.Path = path;
            package.MstPath = mstPath;
            package.SystemType = systemType;
            package.ImpersonateToCurrentLoggedUser = impersonate;
            return package;
        }

        public static InstallPackageInfo CreateExePackageInfo(string path, string parameters, TargetSystemType systemType, bool impersonate = false)
        {
            var package = new InstallPackageInfo();
            package.PackageType = InstallPackageType.Exe;
            package.Parameters = parameters;
            package.Path = path;
            package.MstPath = null;
            package.SystemType = systemType;
            package.ImpersonateToCurrentLoggedUser = impersonate;
            return package;
        }

        public bool ImpersonateToCurrentLoggedUser { get; private set; }
        public string Parameters { get; private set; }
        public string Path { get; private set; }
        public string MstPath { get; private set; }
        public TargetSystemType SystemType { get; private set; }
        public InstallPackageType PackageType { get; private set; }

        public string[] GetPaths()
        {
            if (!String.IsNullOrWhiteSpace(MstPath))
            {
                return new[] { Path, MstPath };
            }
            else
            {
                return new[] { Path };
            }
        }
    }
}