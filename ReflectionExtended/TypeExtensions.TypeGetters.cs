using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectionExtended
{
    public static partial class TypeExtensions
    {
        public static IEnumerable<Type> GetInheritanceChain(
            this Type type,
            bool includeSelf = true,
            bool includeObject = true
        )
        {
            var currentType = includeSelf ? type : type.BaseType;

            while (currentType is not null && (includeObject || currentType != typeof( object )))
            {
                yield return currentType;

                currentType = currentType.BaseType;
            }
        }

        public static IEnumerable<Type> GetNestedClasses(
            this Type self,
            BindingFlags bindingFlags = BindingFlags.Public,
            bool includeAbstract = true
        ) => from type in self.GetNestedTypes( bindingFlags )
             where type.IsClass
             where includeAbstract || !type.IsAbstract
             select type;

        public static IEnumerable<Type> GetNestedAbstractClasses(
            this Type self,
            BindingFlags bindingFlags = BindingFlags.Public
        ) => from type in self.GetNestedTypes( bindingFlags )
             where type is {IsClass: true, IsAbstract: true}
             select type;

        public static IEnumerable<Type> GetNestedValueTypes(
            this Type self,
            BindingFlags bindingFlags = BindingFlags.Public
        ) => from type in self.GetNestedTypes( bindingFlags ) where type.IsValueType select type;

        public static IEnumerable<Type> GetNestedInterfaces(
            this Type self,
            BindingFlags bindingFlags = BindingFlags.Public
        ) => from type in self.GetNestedTypes( bindingFlags ) where type.IsInterface select type;

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            Assembly assembly = null,
            bool onlyExported = false
        ) => from type in onlyExported
                          ? (assembly ?? self.Assembly).GetExportedTypes()
                          : (assembly ?? self.Assembly).GetTypes()
             where type != self && type.IsAssignableTo( self )
             select type;

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            IEnumerable<Assembly> assemblies,
            bool onlyExported = false
        ) => assemblies.SelectMany( assembly => self.GetDerivedTypes( assembly, onlyExported ) );

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            params Assembly[] assemblies
        ) => self.GetDerivedTypes( (IEnumerable<Assembly>) assemblies );

        public static IEnumerable<Type> GetExportedDerivedTypes(
            this Type self,
            params Assembly[] assemblies
        ) => self.GetDerivedTypes( assemblies, true );

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            AppDomain appDomain,
            bool onlyExported = false
        ) => appDomain.GetAssemblies()
                      .SelectMany( assembly => self.GetDerivedTypes( assembly, onlyExported ) );
    }
}
