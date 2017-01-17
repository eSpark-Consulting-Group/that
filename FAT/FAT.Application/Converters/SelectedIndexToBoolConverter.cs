using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace APTAT.Application.Converters
{
    public class SelectedIndexToBoolConverter : IValueConverter
    {
        public bool IsInverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is int && ((int)value) > -1) ^ IsInverse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
