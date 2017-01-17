using System;
using System.Globalization;
using System.Windows;
using APTAT.Application.MarkupExtensions;

namespace APTAT.Application.Converters
{
    public class BoolToVisibilityConverter2 : ConverterMarkupExtension<BoolToVisibilityConverter2>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverse = parameter != null && parameter.ToString() == "!";

            if ((bool)(value ?? false))
            {
                return !inverse ? Visibility.Visible : Visibility.Collapsed;
            }
            return !inverse ? Visibility.Collapsed : Visibility.Visible;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return (Visibility)value == Visibility.Visible;
            }
            return false;
        }
    }

    public class BoolToOpacityConverter2 : ConverterMarkupExtension<BoolToOpacityConverter2>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverse = parameter != null && parameter.ToString() == "!";

            if ((bool)(value ?? false))
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

    public class BoolToSemiOpacityConverter : ConverterMarkupExtension<BoolToSemiOpacityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverse = parameter != null && parameter.ToString() == "!";

            if ((bool)(value ?? false))
            {
                return !inverse ? 1d : 0.4d;
            }
            return !inverse ? 0.4d : 1d;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolInvertConverter : ConverterMarkupExtension<BoolInvertConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)(value ?? false))
            {
                return false;
            }
            return true;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)(value ?? false))
            {
                return false;
            }
            return true;
        }
    }
}
