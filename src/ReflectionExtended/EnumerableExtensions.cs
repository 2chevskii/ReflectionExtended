using System;
using System.Collections.Generic;
using System.Linq;

namespace ReflectionExtended
{
    public static partial class EnumerableExtensions
    {
        public static IEnumerable<Type> AsTypes<T>(this IEnumerable<T> self) => from type in self select type.GetType();
    }
}
