using System;
using System.Globalization;
using APTAT.Application.MarkupExtensions;

namespace APTAT.Application.Converters
{
    public class IntToOpacityConverter : ConverterMarkupExtension<IntToOpacityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverse = parameter != null && parameter.ToString() == "!";

            if ((int)value >= 1)
            {
                return !inverse ? 1d : 0d;
            }
            return !inverse ? 0d : 1d;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToBoolConverter : ConverterMarkupExtension<IntToBoolConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverse = parameter != null && parameter.ToString() == "!";

            if ((double)value >= 1)
            {
                return !inverse;
            }
            return inverse;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToStringConverter : ConverterMarkupExtension<IntToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value).ToString();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result;
            int.TryParse(value as string, out result);
            return result;
        }
    }
}
