using System;
using System.Collections.Generic;

namespace APTAT.DeploymentCore.Facade.Descent.Commands
{
    public class ComputerComparer : IEqualityComparer<IComputer>
    {
        public bool Equals(IComputer x, IComputer y)
        {
            return x.Name == y.Name && x.DnsName == y.DnsName &&
                   (x.IPAddress?.Equals(y.IPAddress) ?? (y.IPAddress?.Equals(x.IPAddress) ?? true));
        }

        public int GetHashCode(IComputer obj)
        {
            return 327 ^
                   (obj.Name ?? string.Empty).GetHashCode() ^
                   (obj.DnsName ?? string.Empty).GetHashCode() ^
                   (obj.IPAddress != null ? obj.GetHashCode() : 0);
        }
    }
}