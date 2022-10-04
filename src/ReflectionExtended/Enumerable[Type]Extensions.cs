using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace ReflectionExtended
{
    public static partial class EnumerableExtensions
    {
        public static bool AllAre(this IEnumerable<Type> self, Type target) => self.All( type => type.Is( target ) );

        public static bool AllAre<T>(this IEnumerable<Type> self) => self.AllAre( typeof( T ) );

        public static bool AllAreExactly(this IEnumerable<Type> self, Type target) =>
        self.All( type => type.IsExactly( target ) );

        public static bool AllAreExactly<T>(this IEnumerable<Type> self) => self.AllAreExactly( typeof( T ) );

        public static bool AllAreAssignableFrom(this IEnumerable<Type> self, Type source) =>
        self.All( type => type.IsAssignableFrom( source ) );

        public static bool AllAreAssignableFrom<T>(this IEnumerable<Type> self) =>
        self.AllAreAssignableFrom( typeof( T ) );

        public static bool AllAreAssignableTo(this IEnumerable<Type> self, Type target) =>
        self.All( type => type.IsAssignableTo( target ) );

        public static bool AllAreAssignableTo<T>(this IEnumerable<Type> self) => self.AllAreAssignableTo( typeof( T ) );

        public static IEnumerable<Type> WithAttribute(
            this IEnumerable<Type> self,
            Type attributeType,
            bool ignoreInheritance = false,
            bool exactAttributeType = false
        ) => from type in self
             where type.HasAttribute( attributeType, ignoreInheritance, exactAttributeType )
             select type;

        public static IEnumerable<Type> WithAttribute<TAttribute>(
            this IEnumerable<Type> self,
            bool ignoreInheritance = false,
            bool exactAttributeType = false
        ) where TAttribute : Attribute =>
        self.WithAttribute( typeof( TAttribute ), ignoreInheritance, exactAttributeType );

        public static IEnumerable<Type> WithAttributeOnSelf(
            this IEnumerable<Type> self,
            Type attributeType,
            bool exactAttributeType = false
        ) => from type in self where type.HasAttributeOnSelf( attributeType, exactAttributeType ) select type;

        public static IEnumerable<Type> WithAttributeOnSelf<TAttribute>(
            this IEnumerable<Type> self,
            bool exactAttributeType = false
        ) where TAttribute : Attribute => self.WithAttributeOnSelf( typeof( TAttribute ), exactAttributeType );

        public static IEnumerable<Type> Nested(this IEnumerable<Type> self) =>
        from type in self where type.IsNested select type;

        public static IEnumerable<Type> NonNested(this IEnumerable<Type> self) =>
        from type in self where !type.IsNested select type;

        public static IEnumerable<Type> Classes(this IEnumerable<Type> self) => from type in self where type.IsClass select type;

        public static IEnumerable<Type> Structs(this IEnumerable<Type> self) =>
        from type in self where type.IsValueType select type;

        public static IEnumerable<Type> Interfaces(this IEnumerable<Type> self) =>
        from type in self where type.IsInterface select type;

        public static IEnumerable<Type> Abstract(this IEnumerable<Type> self) =>
        from type in self where type.IsAbstract select type;

        public static IEnumerable<Type> NonAbstract(this IEnumerable<Type> self) =>
        from type in self where !type.IsAbstract select type;

        public static IEnumerable<Type> Delegates(this IEnumerable<Type> self) =>
        from type in self where type.IsClass && type.IsSubclassOf( typeof( Delegate ) ) select type;
    }
}
