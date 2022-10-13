using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using JetBrains.Annotations;

namespace ReflectionExtended
{
    public static partial class TypeExtensions
    {
        internal const BindingFlags BINDING_FLAGS_PUBLIC     = BindingFlags.Public;
        internal const BindingFlags BINDING_FLAGS_NON_PUBLIC = BindingFlags.NonPublic;
        internal const BindingFlags BINDING_FLAGS_ALL_ACCESS = BindingFlags.Public | BindingFlags.NonPublic;
        internal const BindingFlags BINDING_FLAGS_INSTANCE   = BindingFlags.Instance;
        internal const BindingFlags BINDING_FLAGS_STATIC     = BindingFlags.Static;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="includeNonPublic"></param>
        /// <returns></returns>
        [NotNull,ItemNotNull]
        public static IEnumerable<FieldInfo> GetConstants([NotNull]this Type self, bool includeNonPublic = false) =>
        from field in self.GetFields(
                                     (includeNonPublic ? BINDING_FLAGS_ALL_ACCESS : BINDING_FLAGS_PUBLIC) |
                                     BINDING_FLAGS_STATIC |
                                     BindingFlags.FlattenHierarchy
                                    )
        where field.IsLiteral && !field.IsInitOnly
        select field;
    }
}
