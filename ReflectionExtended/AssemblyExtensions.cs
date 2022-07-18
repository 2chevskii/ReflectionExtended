using System;
using System.Collections.Generic;
using System.Reflection;

namespace ReflectionExtended
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetClasses(
            this Assembly self,
            bool includeNonPublic = false,
            bool includeNested = false,
            bool includeAbstract = true
        )
        {
            Type[] types;

            if (!includeNonPublic)
            {
                types = self.GetExportedTypes();
            }
            else
            {
                types = self.GetTypes();
            }

            if (types.Length is 0)
                return types;

            List<Type> list = new();

            for (var i = 0; i < types.Length; i++)
            {
                Type type = types[i];

                if (!type.IsClass)
                {
                    continue;
                }

                if (!includeNested && type.DeclaringType is not null)
                {
                    continue;
                }

                if (!includeAbstract && type.IsAbstract)
                {
                    continue;
                }

                list.Add(type);
            }

            return list;
        }

        public static IEnumerable<Type> GetStructs(
            this Assembly self,
            bool includeNonPublic = false,
            bool includeNested = false
        )
        {
            Type[] types;

            if (!includeNonPublic)
            {
                types = self.GetExportedTypes();
            }
            else
            {
                types = self.GetTypes();
            }

            if (types.Length is 0)
                return types;

            List<Type> list = new();

            for (var i = 0; i < types.Length; i++)
            {
                var type = types[i];

                if (!type.IsValueType)
                {
                    continue;
                }

                if (!includeNested && type.DeclaringType is not null)
                {
                    continue;
                }

                list.Add(type);
            }

            return list;
        }

        public static IEnumerable<Type> GetInterfaces(
            this Assembly self,
            bool includeNonPublic = false,
            bool includeNested = false
        )
        {
            Type[] types;

            if (!includeNonPublic)
            {
                types = self.GetExportedTypes();
            }
            else
            {
                types = self.GetTypes();
            }

            if (types.Length is 0)
                return types;

            List<Type> list = new();

            for (var i = 0; i < types.Length; i++)
            {
                var type = types[i];

                if (!type.IsInterface)
                {
                    continue;
                }

                if (!includeNested && type.DeclaringType is not null)
                {
                    continue;
                }

                list.Add(type);
            }

            return list;
        }

        public static IEnumerable<Type> GetTypesWithAttribute(
            this Assembly self,
            Type attributeType,
            bool ignoreInheritance = false,
            bool exactAttributeType = false
        )
        {
            var types = self.GetTypes();

            return types.WithAttribute(attributeType, ignoreInheritance, exactAttributeType);
        }

        public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>(
            this Assembly self,
            bool ignoreInheritance = false,
            bool exactAttributeType = false
        ) where TAttribute : Attribute
        {
            return self.GetTypesWithAttribute(
                typeof(TAttribute),
                ignoreInheritance,
                exactAttributeType
            );
        }

        public static IEnumerable<Type> GetTypesWithAttributeOnSelf(
            this Assembly self,
            Type attributeType,
            bool exactAttributeType = false
        )
        {
            var types = self.GetTypes();

            return types.WithAttributeOnSelf(attributeType, exactAttributeType);
        }

        public static IEnumerable<Type> GetTypesWithAttributeOnSelf<TAttribute>(
            this Assembly self,
            bool exactAttributeType = false
        ) where TAttribute : Attribute
        {
            return self.GetTypesWithAttributeOnSelf(typeof(TAttribute), exactAttributeType);
        }
    }
}
