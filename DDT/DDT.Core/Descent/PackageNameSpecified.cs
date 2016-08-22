using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Messages;

namespace DDT.Core.Descent
{
    public interface IPackageNameSpecified : ISystemKindItem
    {
        bool Is64 { get;  }
        string PackageName { get;  }
    }
    class PackageNameSpecified : SystemKindItem, IPackageNameSpecified
    {
        public bool Is64 { get; }
        public string PackageName { get; }

        public PackageNameSpecified(SystemKind systemKind, string packageName, bool is64, Guid operationUid, Guid targetUid)
            :base (systemKind, operationUid, targetUid)
        {
            Is64 = is64;
            PackageName = packageName;
        }
    }
}
