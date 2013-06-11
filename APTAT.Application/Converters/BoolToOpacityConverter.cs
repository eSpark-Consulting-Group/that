using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace APTAT.Application.Converters
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public Boolean Inverse { get; set; }

        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return 0.0;
            }

            if (Inverse)
            {
                if (value.Equals(true))
                {
                    return 0.0;
                }

                return 1.0;
            }

            if (value.Equals(true))
            {
                return 1.0;
            }

            return 0.0;

        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
