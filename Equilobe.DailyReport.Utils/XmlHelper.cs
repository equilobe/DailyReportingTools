using System;

namespace Equilobe.DailyReport.Utils
{
    public static class XmlHelper
    {
        public static T DeserializeXml<T>(string data)
            where T : new()
        {
            var deserialized = new T();

            Deserialization.XmlDeserialize<T>(data)
                .CopyPropertiesOnObjects(deserialized);

            return deserialized;
        }
    }
}
