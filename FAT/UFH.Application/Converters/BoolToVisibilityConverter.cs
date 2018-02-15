using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UFH.Application.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public Boolean Inverse { get; set; }

        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (Inverse)
            {
                if ((Boolean) value)
                {
                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }

            if ((Boolean) value)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
