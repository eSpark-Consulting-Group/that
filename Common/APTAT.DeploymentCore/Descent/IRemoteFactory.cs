using APTAT.Common;
using APTAT.DeploymentCore.Descent.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.DeploymentCore.Descent
{
    public interface IRemoteObject { }
    public interface IRemoteFactory
    {
        IRemoteHostInfo CreateRemoteHostInfo(Guid targetUid, string hostName, string login,
            string password, bool isLocalComputer);
        IDescentCallback CreateRemoteInstallerJob(IDescentQueryResult queryResult,
            IChangeTrackingSubSystem changeTrackingSubSystem,IScanner scanner,
            IComputer computer);
    }
    public sealed class RemoteFactory : IRemoteFactory
    {
        public IRemoteHostInfo CreateRemoteHostInfo(Guid targetUid, string hostName, string login,
            string password, bool isLocalComputer)
        {
            return new RemoteHostInfo(targetUid, hostName, login, password, isLocalComputer);
        }

        public IDescentCallback CreateRemoteInstallerJob(IDescentQueryResult queryResult, 
            IChangeTrackingSubSystem changeTrackingSubSystem, IScanner scanner, 
            IComputer computer)
        {
            return new RemoteInstallerJob(queryResult, changeTrackingSubSystem, scanner, computer);
        }
    }

}
