using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;
using System.IO;

namespace Valkyrie.WebApiProvider.Extensions
{
    
    public static class NameValueCollectionExt
    {
        public static NameValueCollection ToNameValueCollection(this Object obj)
        {
            var jsonText = JsonConvert.SerializeObject(obj);
            var objects = (Dictionary<string, object>)JTokenToObject(JToken.Parse(jsonText));

            var nameValueCollection = new NameValueCollection(objects.Count);
            foreach (var kvp in objects)
            {
                string value = String.Empty;
                if (kvp.Value != null)
                {
                    var list = kvp.Value as List<Object>;
                    if (list != null)
                    {
                        JArray jArray = new JArray();
                        var sb = new StringBuilder();
                        using (var w = new StringWriter(sb))
                        {
                            using (var jsonWriter = new JsonTextWriter(w))
                            {
                                foreach (var li in list)
                                {
                                    var d = li as Dictionary<string, Object>;
                                    if (d != null)
                                    {
                                        JObject jObject = new JObject();
                                        foreach (var key in d.Keys)
                                            jObject.Add(new JProperty(key, d[key]));
                                        jArray.Add(jObject);
                                    }
                                }
                                jArray.WriteTo(jsonWriter);
                            }
                        }

                        sb.Replace(@"\\", @"\");
                        value = "{" + sb + "}";
                        sb.Clear();
                    }
                    else
                        value = kvp.Value.ToString();
                }
                nameValueCollection.Add(kvp.Key, value);
            }

            return nameValueCollection;
        }

        public static string ToJson(this Object obj)
        {
            var jsonText = JsonConvert.SerializeObject(obj);
            return jsonText;
        }

        private static object JTokenToObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return token.Children<JProperty>().ToDictionary(
                        prop => prop.Name,
                        prop => JTokenToObject(prop.Value));
                case JTokenType.Array:
                    return token.Select(JTokenToObject).ToList();
                default:
                    return ((JValue)token).Value;
            }
        }
    }
}
