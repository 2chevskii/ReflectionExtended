﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace ReflectionExtended
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Check if current type is assignable to given type
        /// </summary>
        /// <param name="self">Current type</param>
        /// <param name="other">Target type</param>
        /// <returns></returns>
        public static bool Is(this Type self, Type other)
        {
            return other.IsAssignableFrom(self);
        }

        /// <summary>
        /// <inheritdoc cref="Is"/>
        /// </summary>
        /// <typeparam name="T">Current type</typeparam>
        /// <param name="self">Target type</param>
        /// <returns></returns>
        public static bool Is<T>(this Type self)
        {
            return self.Is(typeof(T));
        }

        /// <summary>
        /// Check if current type is equal to given type
        /// </summary>
        /// <param name="self">Current type</param>
        /// <param name="other">Target type</param>
        /// <returns></returns>
        public static bool IsExactly(this Type self, Type other)
        {
            return self == other;
        }

        /// <summary>
        /// <inheritdoc cref="IsExactly"/>
        /// </summary>
        /// <typeparam name="T">Current type</typeparam>
        /// <param name="self">Given type</param>
        /// <returns></returns>
        public static bool IsExactly<T>(this Type self)
        {
            return self.IsExactly(typeof(T));
        }

        /// <summary>
        /// Generic overload of <see cref="Type.IsAssignableFrom"/>
        /// </summary>
        /// <typeparam name="T">Source type</typeparam>
        /// <param name="self">Current type</param>
        /// <returns></returns>
        public static bool IsAssignableFrom<T>(this Type self)
        {
            return self.IsAssignableFrom(typeof(T));
        }

        public static bool IsAssignableTo<T>(this Type self)
        {
            return self.IsAssignableTo(typeof(T));
        }

#if !NET5_0_OR_GREATER
        /// <summary>
        /// Polyfill for .NET &lt; 5 which does the same as <see cref="Type.IsAssignableFrom"/> but with swapped arguments
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsAssignableTo(this Type self, Type other)
        {
            return other.IsAssignableFrom(self);
        }

#endif

        public static bool IsEnumerable(this Type self, bool notString = true)
        {
            if (notString && self == typeof(string))
                return false;

            return typeof(IEnumerable).IsAssignableFrom(self);
        }

        public static bool IsGenericCollection(this Type self)
        {
            return typeof(ICollection<>).IsAssignableFrom(self);
        }

        public static bool IsNonGenericCollection(this Type self)
        {
            return typeof(ICollection).IsAssignableFrom(self);
        }

        public static bool IsGenericList(this Type self)
        {
            return typeof(IList<>).IsAssignableFrom(self);
        }

        public static bool IsNonGenericList(this Type self)
        {
            return typeof(IList).IsAssignableFrom(self);
        }

        public static IEnumerable<Type> GetInheritanceChain(this Type type, bool includeSelf = true)
        {
            var currentType = includeSelf ? type : type.BaseType;

            while (currentType is not null)
            {
                yield return currentType;

                currentType = type.BaseType;
            }
        }
    }

}