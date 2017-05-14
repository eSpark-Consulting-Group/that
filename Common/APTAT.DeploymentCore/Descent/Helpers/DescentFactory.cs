using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.DeploymentCore.Descent.Helpers
{
    public interface IDescentFactory
    {
        IDescentImpersonationHelper CreateImpersonationHelper();
    }
    public class DescentFactory : IDescentFactory
    {
        private readonly INetworkObserver _networkHelper;
        public DescentFactory(INetworkObserver networkHelper)
        {
            _networkHelper = networkHelper;
        }
        public IDescentImpersonationHelper CreateImpersonationHelper()
        {
            return new DescentImpersonationHelper(_networkHelper);
        }
    }
}
