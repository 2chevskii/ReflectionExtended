using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace ReflectionExtended
{
    /// <summary>
    /// 
    /// </summary>
    [PublicAPI]
    public static partial class EnumerableExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool AllAre(
            [NotNull, ItemNotNull] this IEnumerable<Type> self,
            [NotNull]                   Type              target
        ) => self.All( type => type.Is( target ) );

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool AllAre<T>([NotNull, ItemNotNull] this IEnumerable<Type> self) =>
        self.AllAre( typeof( T ) );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool AllAreExactly(
            [NotNull, ItemNotNull] this IEnumerable<Type> self,
            [NotNull]                   Type              target
        ) => self.All( type => type.IsExactly( target ) );

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool AllAreExactly<T>([NotNull, ItemNotNull] this IEnumerable<Type> self) =>
        self.AllAreExactly( typeof( T ) );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool AllAreAssignableFrom(
            [NotNull, ItemNotNull] this IEnumerable<Type> self,
            [NotNull]                   Type              source
        ) => self.All( type => type.IsAssignableFrom( source ) );

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool AllAreAssignableFrom<T>(
            [NotNull, ItemNotNull] this IEnumerable<Type> self
        ) => self.AllAreAssignableFrom( typeof( T ) );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool AllAreAssignableTo(
            [NotNull, ItemNotNull] this IEnumerable<Type> self,
            [NotNull]                   Type              target
        ) => self.All( type => type.IsAssignableTo( target ) );

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool
        AllAreAssignableTo<T>([NotNull, ItemNotNull] this IEnumerable<Type> self) =>
        self.AllAreAssignableTo( typeof( T ) );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="attributeType"></param>
        /// <param name="ignoreInheritance"></param>
        /// <param name="exactAttributeType"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> WithAttribute(
            [NotNull, ItemNotNull] this IEnumerable<Type> self,
            [NotNull]                   Type              attributeType,
            bool                                          ignoreInheritance  = false,
            bool                                          exactAttributeType = false
        ) => from type in self
             where type.HasAttribute( attributeType, ignoreInheritance, exactAttributeType )
             select type;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="self"></param>
        /// <param name="ignoreInheritance"></param>
        /// <param name="exactAttributeType"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> WithAttribute<TAttribute>(
            [NotNull, ItemNotNull] this IEnumerable<Type> self,
            bool                                          ignoreInheritance  = false,
            bool                                          exactAttributeType = false
        ) where TAttribute : Attribute => self.WithAttribute(
            typeof( TAttribute ),
            ignoreInheritance,
            exactAttributeType
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="attributeType"></param>
        /// <param name="exactAttributeType"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> WithAttributeOnSelf(
            [NotNull, ItemNotNull] this IEnumerable<Type> self,
            [NotNull]                   Type              attributeType,
            bool                                          exactAttributeType = false
        ) => from type in self
             where type.HasAttributeOnSelf( attributeType, exactAttributeType )
             select type;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="self"></param>
        /// <param name="exactAttributeType"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> WithAttributeOnSelf<TAttribute>(
            [NotNull, ItemNotNull] this IEnumerable<Type> self,
            bool                                          exactAttributeType = false
        ) where TAttribute : Attribute => self.WithAttributeOnSelf(
            typeof( TAttribute ),
            exactAttributeType
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type>
        Nested([NotNull, ItemNotNull] this IEnumerable<Type> self) =>
        from type in self where type.IsNested select type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> NonNested(
            [NotNull, ItemNotNull] this IEnumerable<Type> self
        ) => from type in self where !type.IsNested select type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type>
        Classes([NotNull, ItemNotNull] this IEnumerable<Type> self) =>
        from type in self where type.IsClass select type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type>
        Structs([NotNull, ItemNotNull] this IEnumerable<Type> self) =>
        from type in self where type.IsValueType select type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> Interfaces(
            [NotNull, ItemNotNull] this IEnumerable<Type> self
        ) => from type in self where type.IsInterface select type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type>
        Abstract([NotNull, ItemNotNull] this IEnumerable<Type> self) =>
        from type in self where type.IsAbstract select type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> NonAbstract(
            [NotNull, ItemNotNull] this IEnumerable<Type> self
        ) => from type in self where !type.IsAbstract select type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> Delegates(
            [NotNull, ItemNotNull] this IEnumerable<Type> self
        ) => from type in self
             where type.IsClass && type.IsSubclassOf( typeof( Delegate ) )
             select type;
    }
}
