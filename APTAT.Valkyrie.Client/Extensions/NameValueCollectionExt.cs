using Newtonsoft.Json;
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
            var jsonText = JsonConvert.SerializeObject(obj);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);
            var nameValueCollection = new NameValueCollection(dict.Count);
            foreach (var kvp in dict)
            {
                nameValueCollection.Add(kvp.Key.ToString(), kvp.Value.ToString());
            }
            return nameValueCollection;
        }
    }
}
