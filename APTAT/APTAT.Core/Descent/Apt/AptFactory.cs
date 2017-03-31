using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;

namespace APTAT.Core.Descent.Apt
{
    public sealed class AptFactory : IAptFactory
    {
        private readonly IComputerDescentResultFactory _computerDescentFactory;
        private readonly IDescentWatcherFactory _descentWatcherFactory;
        public AptFactory(IComputerDescentResultFactory computerDescentFactory,
            IDescentWatcherFactory descentWatcherFactory)
        {
            _descentWatcherFactory = descentWatcherFactory;
            _computerDescentFactory = computerDescentFactory;
        }
        public IDescentQueryResult CreateContext(IEnumerable<IComputer> computers, IDevicesSourceDescription deviceSourceDescription)
        {
            var result = 
                new AptScanOperationContext(computers, deviceSourceDescription, _computerDescentFactory, _descentWatcherFactory);
            return result;
        }
        public IAptDescentMod CreateDescentMode(TaskFactory taskFactory, IChangeTrackingSubSystem changeTrackingSubSystem)
        {
            var descentMod = new AptDescentMod(taskFactory, changeTrackingSubSystem);
            return descentMod;
        }
    }

}