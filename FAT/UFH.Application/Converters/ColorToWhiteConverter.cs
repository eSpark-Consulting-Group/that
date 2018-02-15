using UFH.Application.MarkupExtensions;
using System;
using System.Globalization;
using System.Windows.Media;

namespace UFH.Application.Converters
{
    public class ColorToWhiteConverter : ConverterMarkupExtension<ColorToWhiteConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((SolidColorBrush)value).Color == Colors.White)
            {
                return value;
            }
            return new SolidColorBrush(Colors.Black);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
