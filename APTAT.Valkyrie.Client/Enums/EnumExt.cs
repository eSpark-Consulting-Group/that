using System;
using System.ComponentModel;
using System.Reflection;

namespace Valkyrie.WebApiProvider.Enums
{
    public static class EnumExt
    {
        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi == null) throw new InvalidOperationException("Not supported enum value " + value);
            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes.Length > 0)
                return attributes[0].Description;
            return value.ToString();
        }
    }
}
