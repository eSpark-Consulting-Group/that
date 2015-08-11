using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.Client
{
    public static class NameValueCollectionExt
    {
        public static NameValueCollection ToNameValueCollection(this Object obj)
        {
            var properties = obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
.ToDictionary(prop => prop.Name, prop => prop.GetValue(obj, null));


            NameValueCollection result = new NameValueCollection();
            foreach (var item in properties)
            {
                result.Add(item.Key.ToLower(), item.Value.ToString());

            }
            return result;
        }
    }
}
