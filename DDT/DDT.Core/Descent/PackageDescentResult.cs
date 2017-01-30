using System.Collections.Generic;
using APTAT.Common;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using System;

namespace DDT.Core.Descent
{
    class PackageDescentResult : DescentResult  
    {
        public PackageDescentResult(IEnumerable<IComputer> computers, IDevicesSourceDescription devicesSourceDescription) 
            : base(computers, devicesSourceDescription, new PackageComputerDescentResultFactory())
        {
        }

        protected sealed class PackageComputerDescentResultFactory : IComputerDescentResultFactory
        {
            public IComputerDescentResultInternal CreateComputerDescentResult(IComputer computer, Guid operationUid)
            {
                return new PackageComputerDescentResult(computer, operationUid);
            }
        }
        public override void Changed(params ChangesTrackingItem[] items)
        {
            base.Changed(items);
            foreach (var item in items)
            {
                if (item.ChangeType == ChangeType.Changed && item.ChangedItem is ISystemKindItem)
                {
                    ISystemKindItem itemWithSysteKind = (ISystemKindItem)item.ChangedItem;
                    if (itemWithSysteKind.OperationUid == OperationUid && ResultsByUid.ContainsKey(itemWithSysteKind.TargetUid))
                    {
                        IComputerDescentResultInternal originalResult = ResultsByUid[itemWithSysteKind.TargetUid];
                        if (originalResult is IPackageComputerDescentResultInternal)
                        {
                            ((IPackageComputerDescentResultInternal)originalResult).UpdateResult(itemWithSysteKind);
                        }
                        else if (originalResult.Computer.SystemKind != itemWithSysteKind.SystemKind)
                        {
                            originalResult.SetSystemKind(itemWithSysteKind.SystemKind);
                        }
                    }
                }
            }
        }
    }
}
