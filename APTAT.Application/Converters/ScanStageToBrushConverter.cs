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

        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            ScanStage item = (ScanStage)value;


            if (item == ScanStage.OperationCompleted)
            {
                return CompletedBrush;
            }

            if (item == ScanStage.OperationFailed)
            {
                return FailedBrush;
            }

            return ProgressBrush;
        
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
