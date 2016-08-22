using System;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Messages;

namespace DDT.Core.Descent
{
    public interface IPackageComputerDescentResultInternal : IComputerDescentResultInternal, IPackageComputerDescentResult
    {
        void UpdateResult(ISystemKindItem item);
    }

    public interface IPackageComputerDescentResult : IComputerDescentResult
    {
        string PackageName { get;  }
        bool? Is64Bit { get;  }
    }
    public class PackageComputerDescentResult : ComputerDescentResult, IPackageComputerDescentResultInternal
    {
        public string PackageName { get; set; }
        public bool? Is64Bit { get; set; }

        public PackageComputerDescentResult(IComputer computer) : base(computer)
        {
        }
        

        public void UpdateResult(ISystemKindItem item)
        {
            var package = item as IPackageNameSpecified;
            if (package != null)
            {
                PackageName = package.PackageName;
                Is64Bit = package.Is64;
            }
            SetSystemKind(item.SystemKind);
        }
    }
}