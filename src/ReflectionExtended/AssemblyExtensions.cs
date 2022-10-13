using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

namespace ReflectionExtended
{
    /// <summary>
    /// Defines methods helpful for retrieving information about <see cref="Assembly"/> members
    /// </summary>
    [PublicAPI]
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Gets <see cref="Assembly"/>'s publicly available <see cref="Delegate"/> types
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<Type> ExportedDelegates([NotNull] this Assembly self) =>
        from type in self.GetExportedTypes() where type.IsDelegate() select type;

        /// <summary>
        /// Gets all <see cref="Assembly"/>'s <see cref="Delegate"/> types
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<Type> Delegates([NotNull] this Assembly self) =>
        from type in self.GetTypes() where type.IsDelegate() select type;

        /// <summary>
        /// Gets publicly available classes from an <see cref="Assembly"/>
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<Type> ExportedClasses([NotNull] this Assembly self) =>
        from type in self.GetExportedTypes() where type.IsClass && !type.IsDelegate() select type;

        /// <summary>
        /// Gets all classes from an assembly
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<Type> Classes([NotNull] this Assembly self) =>
        from type in self.GetTypes() where type.IsClass && !type.IsDelegate() select type;

        /// <summary>
        /// Gets publicly available structs from an assembly
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<Type> ExportedStructs([NotNull] this Assembly self) =>
        from type in self.GetExportedTypes() where type.IsValueType select type;

        /// <summary>
        /// Gets all structs defined in the given assembly
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<Type> Structs([NotNull] this Assembly self) =>
        from type in self.GetTypes() where type.IsValueType select type;

        /// <summary>
        /// Gets publicly available interfaces defined in the given assembly
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<Type> ExportedInterfaces([NotNull] this Assembly self) =>
        from type in self.GetExportedTypes() where type.IsInterface select type;

        /// <summary>
        /// Gets all interfaces defined in the given assembly
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<Type> Interfaces([NotNull] this Assembly self) =>
        from type in self.GetTypes() where type.IsInterface select type;

        /// <summary>
        /// Gets types which have attribute defined on them
        /// </summary>
        /// <param name="self"></param>
        /// <param name="attributeType"></param>
        /// <param name="ignoreInheritance"></param>
        /// <param name="exactAttributeType"></param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<Type> GetTypesWithAttribute(
            [NotNull] this Assembly self,
            [NotNull]      Type     attributeType,
            bool                    ignoreInheritance  = false,
            bool                    exactAttributeType = false
        ) => self.GetTypes().WithAttribute( attributeType, ignoreInheritance, exactAttributeType );

        /// <summary>
        /// Gets types which have attribute defined on them
        /// </summary>
        /// <param name="self"></param>
        /// <param name="ignoreInheritance"></param>
        /// <param name="exactAttributeType"></param>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>(
            [NotNull] this Assembly self,
            bool                    ignoreInheritance  = false,
            bool                    exactAttributeType = false
        ) where TAttribute : Attribute => self.GetTypesWithAttribute(
            typeof( TAttribute ),
            ignoreInheritance,
            exactAttributeType
        );

        /// <summary>
        /// Gets types which have attribute defined on them directly
        /// </summary>
        /// <param name="self"></param>
        /// <param name="attributeType"></param>
        /// <param name="exactAttributeType"></param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<Type> GetTypesWithAttributeOnSelf(
            [NotNull] this Assembly self,
            [NotNull]      Type     attributeType,
            bool                    exactAttributeType = false
        ) => self.GetTypes().WithAttributeOnSelf( attributeType, exactAttributeType );

        /// <summary>
        /// Gets types which have attribute defined on them directly
        /// </summary>
        /// <param name="self"></param>
        /// <param name="exactAttributeType"></param>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<Type> GetTypesWithAttributeOnSelf<TAttribute>(
            [NotNull] this Assembly self,
            bool                    exactAttributeType = false
        ) where TAttribute : Attribute => self.GetTypesWithAttributeOnSelf(
            typeof( TAttribute ),
            exactAttributeType
        );
    }
}
