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

            while (currentType is not null && (includeObject || currentType != typeof(object)))
            {
                yield return currentType;

                currentType = currentType.BaseType;
            }
        }

        public static IEnumerable<Type> GetNestedClasses(
            this Type self,
            BindingFlags bindingFlags = BindingFlags.Public,
            bool includeAbstract = true
        )
        {
            return self.GetNestedTypes(bindingFlags)
                       .Where(t => t.IsClass && (includeAbstract || !t.IsAbstract));
        }

        public static IEnumerable<Type> GetNestedAbstractClasses(
            this Type self,
            BindingFlags bindingFlags = BindingFlags.Public
        )
        {
            return self.GetNestedTypes(bindingFlags).Where(t => t.IsClass && t.IsAbstract);
        }

        public static IEnumerable<Type> GetNestedValueTypes(
            this Type self,
            BindingFlags bindingFlags = BindingFlags.Public
        )
        {
            return self.GetNestedTypes(bindingFlags).Where(t => t.IsValueType);
        }

        public static IEnumerable<Type> GetNestedInterfaces(
            this Type self,
            BindingFlags bindingFlags = BindingFlags.Public
        )
        {
            return self.GetNestedTypes(bindingFlags).Where(t => t.IsInterface);
        }

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            Assembly assembly,
            bool onlyExported = false
        )
        {
            Type[] assemblyTypes;

            if (onlyExported)
            {
                assemblyTypes = assembly.GetExportedTypes();
            }
            else
            {
                assemblyTypes = assembly.GetTypes();
            }

            return assemblyTypes.Where(t => t.Is(self) && t != self);
        }

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            IEnumerable<Assembly> assemblies,
            bool onlyExported = false
        )
        {
            return assemblies.SelectMany(assembly => self.GetDerivedTypes(assembly, onlyExported));
        }

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            params Assembly[] assemblies
        )
        {
            return self.GetDerivedTypes((IEnumerable<Assembly>) assemblies);
        }

        public static IEnumerable<Type> GetExportedDerivedTypes(
            this Type self,
            params Assembly[] assemblies
        )
        {
            return self.GetDerivedTypes(assemblies, true);
        }

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            AppDomain appDomain,
            bool onlyExported = false
        )
        {
            return appDomain.GetAssemblies()
                            .SelectMany(assembly => self.GetDerivedTypes(assembly, onlyExported));
        }
    }
}
