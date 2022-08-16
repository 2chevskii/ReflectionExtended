using System;
using System.Collections.Generic;
using System.Linq;
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
        ) => from type in includeNonPublic ? self.GetTypes() : self.GetExportedTypes()
             where type.IsClass
             where includeNested || type.DeclaringType is null
             where includeAbstract || !type.IsAbstract
             select type;

        public static IEnumerable<Type> GetStructs(
            this Assembly self,
            bool includeNonPublic = false,
            bool includeNested = false
        ) => from type in includeNonPublic ? self.GetTypes() : self.GetExportedTypes()
             where type.IsValueType
             where includeNested || type.DeclaringType is null
             select type;

        public static IEnumerable<Type> GetInterfaces(
            this Assembly self,
            bool          includeNonPublic = false,
            bool          includeNested    = false
        ) => from type in includeNonPublic ? self.GetTypes() : self.GetExportedTypes()
             where type.IsInterface
             where includeNested || type.DeclaringType is null
             select type;

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
