using System;

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
    }
}
