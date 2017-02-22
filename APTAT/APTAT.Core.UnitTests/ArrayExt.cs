using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Core.UnitTests
{
    public static class ArrayExt
    {
        public static bool Compare(this IEnumerable<string> array1, IEnumerable<string> array2)
        {
            return array1.Count() == array2.Count() && array1.Intersect(array2).Count() == array1.Count();                       
        }
    }
}
