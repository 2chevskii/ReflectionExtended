using System;
using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;

// ReSharper disable MethodNameNotMeaningful

namespace ReflectionExtended
{
    public static partial class TypeExtensions
    {
        /// <summary>
        /// Check if current type is assignable to given type
        /// </summary>
        /// <param name="self">Current type</param>
        /// <param name="other">Target type</param>
        /// <returns></returns>
        public static bool Is([NotNull] this Type self, [NotNull] Type other) =>
        other.IsAssignableFrom( self );

        /// <summary>
        /// <inheritdoc cref="Is"/>
        /// </summary>
        /// <typeparam name="T">Current type</typeparam>
        /// <param name="self">Target type</param>
        /// <returns></returns>
        public static bool Is<T>([NotNull] this Type self) => self.Is( typeof( T ) );

        /// <summary>
        /// Check if current type is equal to given type
        /// </summary>
        /// <param name="self">Current type</param>
        /// <param name="other">Target type</param>
        /// <returns></returns>
        public static bool IsExactly([NotNull] this Type self, [NotNull] Type other) =>
        self == other;

        /// <summary>
        /// <inheritdoc cref="IsExactly"/>
        /// </summary>
        /// <typeparam name="T">Current type</typeparam>
        /// <param name="self">Given type</param>
        /// <returns></returns>
        public static bool IsExactly<T>([NotNull] this Type self) => self == typeof( T );

        /// <summary>
        /// Generic overload of <see cref="Type.IsAssignableFrom"/>
        /// </summary>
        /// <typeparam name="T">Source type</typeparam>
        /// <param name="self">Current type</param>
        /// <returns></returns>
        public static bool IsAssignableFrom<T>([NotNull] this Type self) =>
        self.IsAssignableFrom( typeof( T ) );

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsAssignableTo<T>([NotNull] this Type self) =>
        self.IsAssignableTo( typeof( T ) );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsDelegate([NotNull] this Type self) =>
        self.IsClass && self.IsSubclassOf( typeof( Delegate ) );

#if !NET5_OR_GREATER

        /// <summary>
        /// Polyfill for .NET &lt; 5 which does the same as <see cref="Type.IsAssignableFrom"/> but with swapped arguments
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsAssignableTo([NotNull] this Type self, [NotNull] Type other) =>
        other.IsAssignableFrom( self );

#endif

#region Common known types checks

        // ReSharper disable MissingAnnotation
        static readonly Type s_StringType            = typeof( string );
        static readonly Type s_EnumerableType        = typeof( IEnumerable );
        static readonly Type s_EnumerableGenericType = typeof( IEnumerable<> );
        static readonly Type s_CollectionType        = typeof( ICollection );
        static readonly Type s_CollectionGenericType = typeof( ICollection<> );
        static readonly Type s_ListType              = typeof( IList );
        static readonly Type s_ListGenericType       = typeof( IList<> );
        // ReSharper restore MissingAnnotation

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="notString"></param>
        /// <returns></returns>
        public static bool IsEnumerable([NotNull] this Type self, bool notString = true)
        {
            if (notString && self == s_StringType)
                return false;

            return s_EnumerableType.IsAssignableFrom( self );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="notString"></param>
        /// <returns></returns>
        public static bool IsEnumerableGeneric([NotNull] this Type self, bool notString = true)
        {
            if (notString && self == typeof( string ))
                return false;

            return self.GetInterface( s_EnumerableGenericType.Name ) != null ||
                   self.IsGenericType &&
                   s_EnumerableGenericType.IsAssignableFrom( self.GetGenericTypeDefinition() );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsCollection([NotNull] this Type self)
        {
            return s_CollectionType.IsAssignableFrom( self );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsGenericCollection([NotNull] this Type self)
        {
            return self.GetInterface( s_CollectionGenericType.Name ) != null ||
                   self.IsGenericType &&
                   s_CollectionGenericType.IsAssignableFrom( self.GetGenericTypeDefinition() );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsList([NotNull] this Type self)
        {
            return s_ListType.IsAssignableFrom( self );
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsGenericList([NotNull] this Type self)
        {
            return self.GetInterface( s_ListGenericType.Name ) != null ||
                   self.IsGenericType &&
                   s_ListGenericType.IsAssignableFrom( self.GetGenericTypeDefinition() );
        }

#endregion
    }
}
