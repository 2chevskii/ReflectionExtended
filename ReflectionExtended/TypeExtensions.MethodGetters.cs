using System.Reflection;
using System;

namespace ReflectionExtended
{
    public static partial class TypeExtensions
    {
        public static MethodInfo GetInstanceMethod(this Type self, string name, bool includeNonPublic = false) =>
        self.GetMethod(
                       name,
                       BINDING_FLAGS_INSTANCE | (includeNonPublic ? BINDING_FLAGS_ALL_ACCESS : BINDING_FLAGS_PUBLIC)
                      );

        public static MethodInfo GetStaticMethod(this Type self, string name, bool includeNonPublic = false) =>
        self.GetMethod(
                       name,
                       BINDING_FLAGS_STATIC | (includeNonPublic ? BINDING_FLAGS_ALL_ACCESS : BINDING_FLAGS_PUBLIC)
                      );
    }
}
