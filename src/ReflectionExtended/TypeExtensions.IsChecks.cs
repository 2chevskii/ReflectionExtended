using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;

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
        public static bool Is(this Type self, Type other) => other.IsAssignableFrom( self );

        /// <summary>
        /// <inheritdoc cref="Is"/>
        /// </summary>
        /// <typeparam name="T">Current type</typeparam>
        /// <param name="self">Target type</param>
        /// <returns></returns>
        public static bool Is<T>(this Type self) => self.Is( typeof( T ) );

        /// <summary>
        /// Check if current type is equal to given type
        /// </summary>
        /// <param name="self">Current type</param>
        /// <param name="other">Target type</param>
        /// <returns></returns>
        public static bool IsExactly(this Type self, Type other) => self == other;

        /// <summary>
        /// <inheritdoc cref="IsExactly"/>
        /// </summary>
        /// <typeparam name="T">Current type</typeparam>
        /// <param name="self">Given type</param>
        /// <returns></returns>
        public static bool IsExactly<T>(this Type self) => self == typeof( T );

        /// <summary>
        /// Generic overload of <see cref="Type.IsAssignableFrom"/>
        /// </summary>
        /// <typeparam name="T">Source type</typeparam>
        /// <param name="self">Current type</param>
        /// <returns></returns>
        public static bool IsAssignableFrom<T>(this Type self) =>
        self.IsAssignableFrom( typeof( T ) );

        public static bool IsAssignableTo<T>(this Type self) => self.IsAssignableTo( typeof( T ) );

        public static bool IsDelegate(this Type self) =>
        self.IsClass && self.IsSubclassOf( typeof( Delegate ) );

        #if !NET5_OR_GREATER

        /// <summary>
        /// Polyfill for .NET &lt; 5 which does the same as <see cref="Type.IsAssignableFrom"/> but with swapped arguments
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsAssignableTo(this Type self, Type other) =>
        other.IsAssignableFrom( self );

        #endif

#region Common known types checks

        static readonly Type StringType            = typeof( string );
        static readonly Type EnumerableType        = typeof( IEnumerable );
        static readonly Type EnumerableGenericType = typeof( IEnumerable<> );
        static readonly Type CollectionType        = typeof( ICollection );
        static readonly Type CollectionGenericType = typeof( ICollection<> );
        static readonly Type ListType              = typeof( IList );
        static readonly Type ListGenericType       = typeof( IList<> );

        public static bool IsEnumerable(this Type self, bool notString = true)
        {
            if (notString && self == StringType)
                return false;

            return EnumerableType.IsAssignableFrom( self );
        }

        public static bool IsEnumerableGeneric(this Type self, bool notString = true)
        {
            if (notString && self == typeof( string ))
                return false;

            return self.GetInterface( EnumerableGenericType.Name ) is not null ||
                   self.IsGenericType &&
                   EnumerableGenericType.IsAssignableFrom( self.GetGenericTypeDefinition() );
        }

        public static bool IsCollection(this Type self)
        {
            return CollectionType.IsAssignableFrom( self );
        }

        public static bool IsGenericCollection(this Type self)
        {
            return self.GetInterface( CollectionGenericType.Name ) is not null ||
                   self.IsGenericType &&
                   CollectionGenericType.IsAssignableFrom( self.GetGenericTypeDefinition() );
        }

        public static bool IsList(this Type self)
        {
            return ListType.IsAssignableFrom( self );
        }

        public static bool IsGenericList(this Type self)
        {
            return self.GetInterface( ListGenericType.Name ) is not null ||
                   self.IsGenericType &&
                   ListGenericType.IsAssignableFrom( self.GetGenericTypeDefinition() );
        }

#endregion
    }
}
