using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.DAL
{
  public static   class DateTimeExt
    {
        public static DateTime? MinValueToNull(this DateTime? dateTime)
        {
            if (dateTime == DateTime.MinValue) return null;
            return dateTime;
        }
    }
}
