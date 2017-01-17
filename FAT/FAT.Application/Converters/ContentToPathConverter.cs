using APTAT.Application.MarkupExtensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace APTAT.Application.Converters
{
    public class ContentToPathConverter : ConverterMarkupExtension<ContentToPathConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var ps = new PathSegmentCollection(3);
            ContentPresenter cp = (ContentPresenter)value;
            double h = 30;
            double w = 1.15 * cp.ActualWidth;
            ps.Add(new LineSegment(new Point(0, h), true));
            ps.Add(new LineSegment(new Point(w, h), true));
            ps.Add(new BezierSegment(new Point(w + 0.6 * h, h), new Point(w + h, 0), new Point(w + h, 0), true));
            return ps;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}

