using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ReflectionExtended
{
    public static partial class TypeExtensions
    {
        const BindingFlags BINDING_FLAGS_PUBLIC     = BindingFlags.Public;
        const BindingFlags BINDING_FLAGS_NON_PUBLIC = BindingFlags.NonPublic;
        const BindingFlags BINDING_FLAGS_ALL_ACCESS = BindingFlags.Public | BindingFlags.NonPublic;
        const BindingFlags BINDING_FLAGS_INSTANCE   = BindingFlags.Instance;
        const BindingFlags BINDING_FLAGS_STATIC     = BindingFlags.Static;
        
        public static IEnumerable<FieldInfo> GetConstants(
            this Type self,
            bool includeNonPublic = false
        )
        {
            BindingFlags bindingFlags =
                (includeNonPublic ? BINDING_FLAGS_ALL_ACCESS : BINDING_FLAGS_PUBLIC) |
                BINDING_FLAGS_STATIC |
                BindingFlags.FlattenHierarchy;

            return from field in self.GetFields(bindingFlags)
                   where field.IsLiteral && !field.IsInitOnly
                   select field;
        }
    }
}
