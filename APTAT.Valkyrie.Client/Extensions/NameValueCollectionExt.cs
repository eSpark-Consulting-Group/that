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
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonText);
            var nameValueCollection = new NameValueCollection(dict.Count);
            foreach (var kvp in dict)
            {
                string value = String.Empty;
                if (kvp.Value != null)
                {
                    value = kvp.Value.ToString();
                }

                nameValueCollection.Add(kvp.Key, value);
            }
            return nameValueCollection;
        }
    }
}
