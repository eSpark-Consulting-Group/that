using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using APTAT.Application.Models;

namespace APTAT.Application.Converters
{
    public class ScanStageToBrushConverter : IValueConverter
    {
        public Brush OfflineBrush { get; set; }

        public Brush ProgressBrush { get; set; }

        public Brush CompletedBrush { get; set; }

        public Brush FailedBrush { get; set; }

        public Brush CanceledBrush { get; set; }

        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            ScanStatus item = (ScanStatus)value;


            if (item.Equals(new ScanStatus(ScanStatusEnum.Completed)))
            {
                return CompletedBrush;
            }

            if (item.Equals(new ScanStatus(ScanStatusEnum.Failed)))
            {
                return FailedBrush;
            }

            if (item.Equals(new ScanStatus(ScanStatusEnum.Cancelled)))
            {
                return CanceledBrush;
            }

            return ProgressBrush;
        
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
