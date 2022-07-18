using System.Collections.Generic;
using System;
using System.Linq;

namespace ReflectionExtended
{
    public static partial class EnumerableExtensions
    {
        public static IEnumerable<Type> ToTypeEnumerable<T>(this IEnumerable<T> self)
        {
            return self.Select(obj => obj.GetType());
        }
    }
}
