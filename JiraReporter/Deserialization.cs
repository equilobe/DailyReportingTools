using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JiraReporter
{
    public class Serialization
    {
        public static string XmlSerialize(object obj)
        {
            var writer = new StringWriter();
            var serializer = new XmlSerializer(obj.GetType());
            serializer.Serialize(writer, obj);

            return writer.ToString();
        }
    }

    class Deserialization
    {
        public static T XmlDeserialize <T>(string xmlString)
        {
            StringReader reader = new StringReader(xmlString);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(reader);
        }

        public static T JsonDeserialize<T>(string jsonString)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            return (T)ser.ReadObject(stream);
        }
    }
}
