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
    public interface ISystemKindItem : IBusinessObject
    {
        SystemKind SystemKind { get; }
        Guid OperationUid { get; }
        Guid TargetUid { get; }
    }
    public class SystemKindItem : ISystemKindItem
    {
        public SystemKind SystemKind { get; }
        public Guid OperationUid { get; }
        public Guid TargetUid { get; }
        public SystemKindItem(SystemKind systemKind, Guid operationUid, Guid targetUid)
        {
            SystemKind = systemKind;
            OperationUid = operationUid;
            TargetUid = targetUid;
        }
    }
}
