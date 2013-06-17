using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using APTAT.Application.Models;

namespace APTAT.Application.Converters
{
    public class ScanStageToOpacityConverter : IValueConverter
    {



    
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            ScanStage item = (ScanStage)value;
            String param = (String) parameter;

            if(param == "OperationCompleted")
            {
                if (item == ScanStage.OperationCompleted)
                {
                    return 1.0;
                }
                return 0.0;
            }
            if (param == "OperationFailed")
            {
                if(item == ScanStage.OperationFailed)
                {
                    return 1.0;
                }
                return 0.0;
            }
            if (param == "OperationCanceled")
            {
                if (item == ScanStage.OperationCanceled)
                {
                    return 1.0;
                }
                return 0.0;
            }

            if (item != ScanStage.OperationCompleted && item != ScanStage.OperationCanceled && item != ScanStage.OperationFailed)
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
