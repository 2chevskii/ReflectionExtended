using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

namespace ReflectionExtended
{
    public static partial class TypeExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="includeSelf"></param>
        /// <param name="includeObject"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> GetInheritanceChain(
            [NotNull] this Type type,
            bool                includeSelf   = true,
            bool                includeObject = true
        )
        {
            Type currentType = includeSelf ? type : type.BaseType;

            while (currentType != null && (includeObject || currentType != typeof( object )))
            {
                yield return currentType;

                currentType = currentType.BaseType;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="bindingFlags"></param>
        /// <param name="includeAbstract"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> GetNestedClasses(
            [NotNull] this Type self,
            BindingFlags        bindingFlags    = BindingFlags.Public,
            bool                includeAbstract = true
        ) => from type in self.GetNestedTypes( bindingFlags )
             where type.IsClass
             where includeAbstract || !type.IsAbstract
             select type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> GetNestedAbstractClasses(
            [NotNull] this Type self,
            BindingFlags        bindingFlags = BindingFlags.Public
        ) => from type in self.GetNestedTypes( bindingFlags )
             where type.IsClass && type.IsAbstract
             select type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> GetNestedValueTypes(
            [NotNull] this Type self,
            BindingFlags        bindingFlags = BindingFlags.Public
        ) => from type in self.GetNestedTypes( bindingFlags ) where type.IsValueType select type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> GetNestedInterfaces(
            [NotNull] this Type self,
            BindingFlags        bindingFlags = BindingFlags.Public
        ) => from type in self.GetNestedTypes( bindingFlags ) where type.IsInterface select type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="assembly"></param>
        /// <param name="includeNonExported"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> GetDerivedTypes(
            [NotNull] this Type     self,
            [NotNull]      Assembly assembly,
            bool                    includeNonExported = true
        ) => from type in includeNonExported ? assembly.GetTypes() : assembly.GetExportedTypes()
             where type != self && type.IsSubclassOf( self )
             select type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="includeNonExported"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> GetDerivedTypes(
            [NotNull] this Type self,
            bool                includeNonExported = true
        ) => self.GetDerivedTypes( self.Assembly, includeNonExported );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="assemblies"></param>
        /// <param name="includeNonExported"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> GetDerivedTypes(
            [NotNull] this         Type                  self,
            [NotNull, ItemNotNull] IEnumerable<Assembly> assemblies,
            bool                                         includeNonExported = true
        ) => assemblies.SelectMany(
            assembly => self.GetDerivedTypes( assembly, includeNonExported )
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> GetDerivedTypes(
            [NotNull] this                Type       self,
            [NotNull, ItemNotNull] params Assembly[] assemblies
        ) => self.GetDerivedTypes( (IEnumerable<Assembly>) assemblies );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> GetExportedDerivedTypes(
            [NotNull] this                Type       self,
            [NotNull, ItemNotNull] params Assembly[] assemblies
        ) => self.GetDerivedTypes( assemblies, false );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="appDomain"></param>
        /// <param name="includeNonExported"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Type> GetDerivedTypes(
            [NotNull] this Type      self,
            [NotNull]      AppDomain appDomain,
            bool                     includeNonExported = true
        )
        {
            return self.GetDerivedTypes( appDomain.GetAssemblies(), includeNonExported );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [NotNull]
        public static TypeTree ToTypeTree([NotNull] this Type self) => TypeTree.Create( self );
    }
}
