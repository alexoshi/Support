using System.Collections.Generic;

namespace BallGame
{
    public static class SerializeClass
    {
        public static string ToXML<T>(this T obj)
        {
            // Remove Declaration
            var settings = new System.Xml.XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            // Remove Namespace
            var ns = new System.Xml.Serialization.XmlSerializerNamespaces(new[] { System.Xml.XmlQualifiedName.Empty });

            using var stream = new System.IO.StringWriter();
            using var writer = System.Xml.XmlWriter.Create(stream, settings);
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            serializer.Serialize(writer, obj, ns);
            return stream.ToString();
        }

        public static T FromXML<T>(string Input)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using System.IO.StringReader reader = new(Input);
            object? v = serializer.Deserialize(reader);

            if (v == null)
                throw new Exception($"Invalid Imput to read from XML: {Input}");

            return (T)v;
        }

    }
}
