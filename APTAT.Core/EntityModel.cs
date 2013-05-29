using System;
using System.Linq;
using System.Collections.Generic;

namespace APTAT.Core
{
    public class Package
    {
        public string Hash { get; set; }
        public string StoredPath { get; set; }
    }

    public class ManagedProduct
    {
        public long Id { get; set; }
        public int SystemKind { get; set; }
        public Guid PluginUid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }

        public virtual ICollection<Package> Packages { get; set; }

        public SystemKind SystemKindAsEnum
        {
            get { return (SystemKind)SystemKind; }
        }

        public string AbbreviatedName
        {
            get
            {
                var initials = Name.Split(default(char[]), StringSplitOptions.RemoveEmptyEntries).Select(w => w[0]);
                return String.Join(null, initials).ToUpperInvariant();
            }
        }
    }
}
