using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DDT.Core.Helper
{
    public static class CSVHelper
    {
        public static string ToCsv<T>(string separator, IEnumerable<T> objectlist)
        {
            Type t = typeof(T);
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

          

            string[] columnNames = new string[properties.Count()];

            for (int i = 0; i < properties.Count(); i++)
            {                
                columnNames[i] = GetNameOrDescription(properties[i]);
            }

            StringBuilder csvdata = new StringBuilder();

            string header = String.Join(separator, columnNames);


            csvdata.AppendLine(header);

            foreach (var o in objectlist)
                csvdata.AppendLine(ToCsvFields(separator, properties, o));

            return csvdata.ToString();
        }

        private static string GetNameOrDescription(PropertyInfo prop)
        {
            object[] attrs = prop.GetCustomAttributes(true);
            if (attrs.Any())
            {
                var desc = attrs[0] as System.ComponentModel.DescriptionAttribute;
                if (desc != null && desc.Description != null)
                    return  desc.Description;
               
            }
            return prop.Name;
        }

        public static string ToCsvFields(string separator, PropertyInfo[] properties, object o)
        {
            StringBuilder linie = new StringBuilder();

            foreach (PropertyInfo prop in properties)
            {
                

                if (linie.Length > 0)
                    linie.Append(separator);

                var x = prop.GetValue(o);

                if (x != null)
                    linie.Append(x.ToString());
            }

            return linie.ToString();
        }
    }
}
