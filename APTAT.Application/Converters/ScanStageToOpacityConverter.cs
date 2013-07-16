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
            ScanStatus item = (ScanStatus)value;
            String param = (String) parameter;

            if(param == "OperationCompleted")
            {
                if (item.Equals(new ScanStatus(ScanStatusEnum.Completed)))
                {
                    return 1.0;
                }
                return 0.0;
            }
            if (param == "OperationFailed")
            {
                if (item.Equals(new ScanStatus(ScanStatusEnum.Failed)))
                {
                    return 1.0;
                }
                return 0.0;
            }
            if (param == "OperationCanceled")
            {
                if (item.Equals(new ScanStatus(ScanStatusEnum.Cancelled)))
                {
                    return 1.0;
                }
                return 0.0;
            }

            if (param == "Offline")
            {
                if (item.Equals(new ScanStatus(ScanStatusEnum.Offline)))
                {
                    return 1.0;
                }

                return 0.0;
            }

            if (param == "InProgress")
            {
                if (item.Equals(new ScanStatus(ScanStatusEnum.InProgress)))
                {
                    return 1.0;
                }

                return 0.0;
            }

            return 0.0;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
