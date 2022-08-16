using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ReflectionExtended
{
    public static partial class TypeExtensions
    {
        public const BindingFlags BINDING_FLAGS_PUBLIC     = BindingFlags.Public;
        public const BindingFlags BINDING_FLAGS_NON_PUBLIC = BindingFlags.NonPublic;
        public const BindingFlags BINDING_FLAGS_ALL_ACCESS = BindingFlags.Public | BindingFlags.NonPublic;
        public const BindingFlags BINDING_FLAGS_INSTANCE   = BindingFlags.Instance;
        public const BindingFlags BINDING_FLAGS_STATIC     = BindingFlags.Static;

        public static IEnumerable<FieldInfo> GetConstants(this Type self, bool includeNonPublic = false) =>
        from field in self.GetFields(
                                     (includeNonPublic ? BINDING_FLAGS_ALL_ACCESS : BINDING_FLAGS_PUBLIC) |
                                     BINDING_FLAGS_STATIC |
                                     BindingFlags.FlattenHierarchy
                                    )
        where field.IsLiteral && !field.IsInitOnly
        select field;
    }
}
