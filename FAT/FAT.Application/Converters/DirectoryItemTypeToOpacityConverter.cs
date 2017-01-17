using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using FAT.Application.Models;

namespace FAT.Application.Converters
{
    public class DirectoryItemTypeToOpacityConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            String p = (String) parameter;
            DirectoryItemType type = (DirectoryItemType) value;
            if(p == "Root")
            {
                if(type == DirectoryItemType.Root)
                {
                    return 1.0;
                }
                return 0.0;
            }
            if(p == "OrgUnit")
            {
                if (type == DirectoryItemType.OrgUnit)
                {
                    return 1.0;
                }
                return 0.0;
            }

            if(p == "Computer")
            {
                if (type == DirectoryItemType.Computer)
                {
                    return 1.0;
                }
                return 0.0;
            }

            return 0.0;
        }

        public object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
