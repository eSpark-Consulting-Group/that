using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Core.Discovery
{
    public interface IDiscoverySubSystem : ISubSystem
    {
        //IDiscoveryProfile[] Profiles { get; }
        //IDictionary<Guid, IDiscoveryProfile[]> ProfilesByProduct { get; }
        //IDiscoveryProfile GetProfile(Guid uid);

        //IDiscoveryData[] GetData(IComputer computer);
        //IDiscoveryData[] GetData(IDiscoveryProfile profile);
        //IDiscoveryData[] GetData(IObjectFilter predicate);

        //event EventHandler<DiscoveryDataEventArgs> Discovered;
    }
}
