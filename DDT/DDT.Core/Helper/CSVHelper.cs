using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace DDT.Core.Helper
{
    class PropertyShortInfo
    {
        internal int Order { get; set; }
        internal string Name { get; set; }
    }

    public static class CSVHelper
    {
        public static string ToCsv<T>(string separator, IEnumerable<T> objectlist)
        {
            Type t = typeof(T);
            PropertyInfo[] ordered = Sort(typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance));

            string[] columnNames = new string[ordered.Count()];


            for (int i = 0; i < ordered.Count(); i++)
            {
                columnNames[i] = GetNameOrDescription(ordered[i]);
            }

            StringBuilder csvdata = new StringBuilder();

            string header = String.Join(separator, columnNames);


            csvdata.AppendLine(header);

            foreach (var o in objectlist)
                csvdata.AppendLine(ToCsvFields(separator, ordered, o));

            return csvdata.ToString();
        }

        private static PropertyInfo[] Sort(PropertyInfo[] properties)
        {
            var list = new List<Tuple<PropertyShortInfo, PropertyInfo>>();

            foreach (var item in properties)
            {
                if (Ignore(item)) continue;
                var info = new Tuple<PropertyShortInfo, PropertyInfo>(GetShortInfo(item), item);
                list.Add(info);
            }

            var result = list.ToLookup((i) => i.Item1, (j) => j.Item2)
                .OrderBy(lk => lk.Key.Order).ThenBy(i=>i.Key.Name).SelectMany(x => x.ToList()).ToArray();

            return result;
        }

        private static bool Ignore(PropertyInfo prop)
        {
            object[] attrs = prop.GetCustomAttributes(true);

            if (attrs != null)
            {
                return attrs.Any(a => a is IgnoreDataMemberAttribute);
            }
            return false;
        }
        private static PropertyShortInfo GetShortInfo(PropertyInfo prop)
        {
            object[] attrs = prop.GetCustomAttributes(true);
            PropertyShortInfo info = new PropertyShortInfo();
            var dataMemberAttr = attrs.Where(a => a is DataMemberAttribute).FirstOrDefault();
            if (dataMemberAttr != null)
            {
                var data = ((DataMemberAttribute) dataMemberAttr);
                info.Order = data.Order;
                info.Name = data.Name;
            
            }
            else
            {
                info.Order = 0;
                info.Name = prop.Name;
            }

            return info;
        }

        private static string GetNameOrDescription(PropertyInfo prop)
        {
            object[] attrs = prop.GetCustomAttributes(true);
            if (attrs.Any())
            {
                var desc = attrs[0] as DataMemberAttribute;
                if (desc != null && desc.Name != null)
                    return desc.Name;

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
