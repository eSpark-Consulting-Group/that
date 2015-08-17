using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.Client
{
    public class ObjectToArrayConverter<T> : CustomCreationConverter<T[]>
    {
        public override T[] Create(Type objectType)
        {
            return new T[0];
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                return serializer.Deserialize(reader, objectType);
            }
            else
            {
                return new T[] { serializer.Deserialize<T>(reader) };
            }
        }
    }
}
