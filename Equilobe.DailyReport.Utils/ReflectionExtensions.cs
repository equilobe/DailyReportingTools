using System;
using System.Linq;
using System.Reflection;

namespace System
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Extension for 'Object' that copies the properties to a destination object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        public static void CopyPropertiesOnObjects(this object source, object destination)
        {
            if (source == null || destination == null)
                return;

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

        public static T CopyTo<T>(this T source, T destination)
        {
            if (source == null)
                throw new ArgumentNullException();

            if (destination == null)
                throw new ArgumentNullException();


            var props = typeof(T)
                        .GetProperties()
                        .Where(p => p.CanWrite);

            foreach (var prop in props)
            {
                prop.SetValue(destination, prop.GetValue(source));
            }

            return source;
        }

        public static TSource CopyTo<TSource, TDest>(this TSource source, TDest destination)
            where TDest : TSource
        {
            source.CopyTo<TSource>(destination);
            return source;
        }

        public static T CopyFrom<T>(this T destination, T source)
        {
            source.CopyTo<T>(destination);
            return destination;
        }

        public static TDest CopyFrom<TDest, TSource>(this TDest destination, TSource source)
            where TDest : TSource
        {
            source.CopyTo(destination);
            return destination;
        }
    }
}