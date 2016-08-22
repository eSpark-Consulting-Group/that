using System;
using System.Globalization;
using System.Windows.Data;

namespace APTAT.Application.Converters
{
    public class NullableConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            return value.Equals(true);
        }

        public object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
