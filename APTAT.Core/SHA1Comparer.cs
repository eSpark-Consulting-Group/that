using System.Collections.Generic;

namespace APTAT.Core
{
    class SHA1Comparer : IEqualityComparer<ValkyrieFileInfo>
    {
        public bool Equals(ValkyrieFileInfo x, ValkyrieFileInfo y)
        {
            return (x.Sha1 == y.Sha1);
        }

        public int GetHashCode(ValkyrieFileInfo obj)
        {
            return obj.Sha1.GetHashCode();
        }
    }
}
