using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Application.Models
{
    class BadFileItemComparer : IEqualityComparer<BadFileItem>
    {
        public bool Equals(BadFileItem x, BadFileItem y)
        {
            return (x.Sha1 == y.Sha1);
        }

        public int GetHashCode(BadFileItem obj)
        {
            return obj.Sha1.GetHashCode();
        }
    }
}
