using System;
using System.Linq;
using System.Reflection;

namespace DailyReportWeb.Services
{
    public static class Reflection
    {
        /// <summary>
        /// Extension for 'Object' that copies the properties to a destination object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        public static void CopyProperties(this object source, object destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");

            var propertyList = from sourceProperty in source.GetType().GetProperties()
                               let targetProperty = destination.GetType().GetProperty(sourceProperty.Name)
                               where sourceProperty.CanRead
                                   && targetProperty != null
                                   && (targetProperty.GetSetMethod(true) != null && !targetProperty.GetSetMethod(true).IsPrivate)
                                   && (targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) == 0
                                   && targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType)
                               select new { sourceProperty = sourceProperty, targetProperty = targetProperty };

            foreach (var property in propertyList)
            {
                property.targetProperty.SetValue(destination, property.sourceProperty.GetValue(source, null), null);
            }
        }

        public static void CopyProperties<T>(this object source, object destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");

            var propertyList = from property in typeof(T).GetProperties()
                               where property.CanRead && property.CanWrite
                                   && property != null
                                   && (property.GetSetMethod(true) != null && !property.GetSetMethod(true).IsPrivate)
                                   && (property.GetSetMethod().Attributes & MethodAttributes.Static) == 0
                               select property;

            foreach (PropertyInfo property in propertyList)
            {
                property.SetValue(destination, property.GetValue(source, null), null);
            }
        }
    }
}