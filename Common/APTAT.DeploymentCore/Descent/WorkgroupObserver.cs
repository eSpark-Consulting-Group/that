using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.Common;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.DeploymentCore.Descent
{
    public interface IWorkgroupObserver : IService
    {
        IAsyncResult BeginSearch(string path, Action<IComputer[]> foundComputersAction, AsyncCallback callback = null);
        void EndSearch(IAsyncResult result);
    }
    public class WorkgroupObserver : IWorkgroupObserver
    {
        private readonly INetworkBrowser _networkBrowser; 
        public WorkgroupObserver(INetworkBrowser networkBrowser)
        {
            _networkBrowser = networkBrowser;
        }
        public IAsyncResult BeginSearch(string path, Action<IComputer[]> foundComputersAction, AsyncCallback callback = null)
        {
            Action action = () =>
            {
                FindComputers(path, foundComputersAction);
            };

            return action.BeginInvoke(callback, action);
        }

        private void FindComputers(string path, Action<IComputer[]> foundComputersAction)
        {
            var computers = new List<string>();
            var result = _networkBrowser.GetNetworkComputers(ServerType.All, path, computers);
            if (result == NetServerEnumResults.ERROR_INVALID_PARAMETER)
            {
                return;
                //throw new FaultException<WorkgroupInvalidFault>( new WorkgroupInvalidFault { WorkgroupName = criteria.WorkgroupName }, "Workgroup name is invalid." );
                //throw new InvalidOperationException("Workgroup name is invalid.");
            }
            else if (result == NetServerEnumResults.ERROR_NO_BROWSER_SERVERS_FOUND)
            {
                return;
                //throw new FaultException<WorkgroupNotFoundFault>( new WorkgroupNotFoundFault { WorkgroupName = criteria.WorkgroupName }, "Workgroup not found." );
                //throw new InvalidOperationException("Workgroup not found.");
            }
            else if (result == NetServerEnumResults.ERROR_MORE_DATA)
            {
                return;
                //throw new FaultException<WorkgroupQueryFault>( new WorkgroupQueryFault { WorkgroupName = criteria.WorkgroupName }, "Too many items are in the workgroup." );
                //throw new InvalidOperationException("Too many items are in the workgroup.");
            }
            else if (result == NetServerEnumResults.UNKNOWN_ERROR)
            {
                return;
                //throw new InvalidOperationException("Unknown error occured.");
            }
            IComputer[] targets = (from name in computers
                select new WorkgroupTarget {Uid = Guid.NewGuid(), Name = name, SmbName = name}).ToArray();
            
            

            if (targets.Length > 0)
            {
                foundComputersAction?.Invoke(targets);
            }
        }

        public void EndSearch(IAsyncResult result)
        {
            Action action = (Action)result.AsyncState;
            action.EndInvoke(result);
        }
    }
}
