using System.Collections.Generic;
using System.Linq;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;

namespace APTAT.Core.Descent.Apt
{
    //public sealed class ContextFactory : IContextFactory
    //{
    //    private readonly IComputerDescentResultFactory _computerDescentFactory;
    //    private readonly IDescentWatcherFactory _descentWatcherFactory;
    //    public ContextFactory(IComputerDescentResultFactory computerDescentFactory,
    //        IDescentWatcherFactory descentWatcherFactory)
    //    {
    //        _descentWatcherFactory = descentWatcherFactory;
    //        _computerDescentFactory = computerDescentFactory;
    //    }
    //    public IDescentQueryResult CreateContext(IEnumerable<IComputer> computers,
    //            IDevicesSourceDescription deviceSourceDesctiption)
    //    {
    //        var result = new AptScanOperationContext(computers, deviceSourceDesctiption,
    //            _computerDescentFactory, _descentWatcherFactory);
    //        return result;
    //    }
    //}

}