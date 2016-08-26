using Xunit;
using DDT.Core.Descent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDT.Core.Descent.Tests
{
    public class InstallPackageInfoTests
    {
        [Fact()]
        public void CreateMsiPackageInfoTest()
        {
            var path = "application.msi";
            var mstPath = "application.mst";
            var parameters = "-silent";

            InstallPackageInfo info = InstallPackageInfo.CreateMsiPackageInfo(path, mstPath, parameters, TargetSystemType.x86);

            Assert.NotNull(info);
            Assert.Equal(path, info.Path);
            Assert.Equal(mstPath, info.MstPath);
            Assert.Equal(parameters, info.Parameters);
            Assert.Equal(TargetSystemType.x86, info.SystemType);
            Assert.Equal(InstallPackageType.Msi, info.PackageType);
            var paths = info.GetPaths();
            Assert.NotNull(paths);
            Assert.Equal(2, paths.Count());
            Assert.Equal(path, paths[0]);
            Assert.Equal(mstPath, paths[1]);
            Assert.Equal(false, info.ImpersonateToCurrentLoggedUser);
        }

        [Fact()]
        public void CreateExePackageInfoTest()
        {
            var path = "application.exe";            
            var parameters = "-silent";

            InstallPackageInfo info = InstallPackageInfo.CreateExePackageInfo(path,  parameters, TargetSystemType.x64);

            Assert.NotNull(info);
            Assert.Equal(path, info.Path);
            Assert.Null(info.MstPath);
            Assert.Equal(parameters, info.Parameters);
            Assert.Equal(TargetSystemType.x64, info.SystemType);
            Assert.Equal(InstallPackageType.Exe, info.PackageType);

            var paths = info.GetPaths();
            Assert.NotNull(paths);
            Assert.Equal(1, paths.Count());
            Assert.Equal(path, paths[0]);
            Assert.Equal(false, info.ImpersonateToCurrentLoggedUser);
        }
    }
}