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
            bool      includeSelf   = true,
            bool      includeObject = true
        )
        {
            Type currentType = includeSelf ? type : type.BaseType;

            while (currentType != null && (includeObject || currentType != typeof( object )))
            {
                yield return currentType;

                currentType = currentType.BaseType;
            }
        }

        public static IEnumerable<Type> GetNestedClasses(
            this Type    self,
            BindingFlags bindingFlags    = BindingFlags.Public,
            bool         includeAbstract = true
        ) => from type in self.GetNestedTypes( bindingFlags )
             where type.IsClass
             where includeAbstract || !type.IsAbstract
             select type;

        public static IEnumerable<Type> GetNestedAbstractClasses(
            this Type    self,
            BindingFlags bindingFlags = BindingFlags.Public
        ) => from type in self.GetNestedTypes( bindingFlags )
             where type.IsClass && type.IsAbstract
             select type;

        public static IEnumerable<Type> GetNestedValueTypes(
            this Type    self,
            BindingFlags bindingFlags = BindingFlags.Public
        ) => from type in self.GetNestedTypes( bindingFlags ) where type.IsValueType select type;

        public static IEnumerable<Type> GetNestedInterfaces(
            this Type    self,
            BindingFlags bindingFlags = BindingFlags.Public
        ) => from type in self.GetNestedTypes( bindingFlags ) where type.IsInterface select type;

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            Assembly  assembly,
            bool      includeNonExported = true
        ) => from type in includeNonExported ? assembly.GetTypes() : assembly.GetExportedTypes()
             where type != self && type.IsSubclassOf( self )
             select type;

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            bool      includeNonExported = true
        ) => self.GetDerivedTypes( self.Assembly, includeNonExported );

        public static IEnumerable<Type> GetDerivedTypes(
            this Type             self,
            IEnumerable<Assembly> assemblies,
            bool                  includeNonExported = true
        ) => assemblies.SelectMany(
            assembly => self.GetDerivedTypes( assembly, includeNonExported )
        );

        public static IEnumerable<Type> GetDerivedTypes(
            this   Type       self,
            params Assembly[] assemblies
        ) => self.GetDerivedTypes( (IEnumerable<Assembly>) assemblies );

        public static IEnumerable<Type> GetExportedDerivedTypes(
            this   Type       self,
            params Assembly[] assemblies
        ) => self.GetDerivedTypes( assemblies, false );

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            AppDomain appDomain,
            bool      includeNonExported = true
        )
        {
            return self.GetDerivedTypes( appDomain.GetAssemblies(), includeNonExported );
        }

        public static TypeTree ToTypeTree(this Type self) => TypeTree.Create( self );
    }
}
