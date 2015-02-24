using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Utils
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

   public class Deserialization
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

        public static T XmlDeserializeFileContent <T>(string filePath)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (var fs = File.OpenRead(filePath))
            {
                var content = (T)xs.Deserialize(fs);
                return content;
            }
        }
    }
}
