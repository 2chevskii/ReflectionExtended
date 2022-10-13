using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace ReflectionExtended
{
    public static partial class EnumerableExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> AsTypes<T>([NotNull] this IEnumerable<T> self) => from item in self select item.GetType();
    }
}
