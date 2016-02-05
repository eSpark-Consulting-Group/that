using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace Valkyrie.WebApiProvider.Extensions
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
                nameValueCollection.Add(kvp.Key.ToString(), (kvp.Value ?? String.Empty).ToString());
            }
            return nameValueCollection;
        }
    }
}
