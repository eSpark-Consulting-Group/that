using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace APTAT.Valkyrie
{

    public static class SerializerHelper<T>
        {
            private static readonly XmlSerializer _Serializer;
            private static readonly XmlWriterSettings _XmlSettings;

            private static readonly XmlSerializerNamespaces _Namespaces;

            static SerializerHelper()
            {
                _Namespaces = new XmlSerializerNamespaces();
                _Namespaces.Add(string.Empty, string.Empty);

                _XmlSettings = new XmlWriterSettings
                {
                    Indent = true,
                    OmitXmlDeclaration = false,
                    Encoding = Encoding.UTF8
                };

                _Serializer = new XmlSerializer(typeof(T));
            }

            public static string ObjectToXml(T obj)
            {
                using (var stringWriter = new StringWriter())
                {
                    using (var xmlWriter = XmlWriter.Create(stringWriter, _XmlSettings))
                    {
                        _Serializer.Serialize(xmlWriter, obj, _Namespaces);
                    }
                    return stringWriter.ToString();
                }
            }
        }

}
