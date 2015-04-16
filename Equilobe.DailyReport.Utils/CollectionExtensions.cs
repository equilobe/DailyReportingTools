using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class CollectionExtensions
    {
        public static bool IsEmpty<T> (this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }
    }
}
