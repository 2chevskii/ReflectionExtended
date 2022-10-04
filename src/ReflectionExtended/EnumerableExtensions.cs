using System;
using System.Collections.Generic;
using System.Linq;

namespace ReflectionExtended
{
    public static partial class EnumerableExtensions
    {
        public static IEnumerable<Type> AsTypes<T>(this IEnumerable<T> self) => from item in self select item.GetType();
    }
}
