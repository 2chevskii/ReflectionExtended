using System.Reflection;
using System;

namespace ReflectionExtended
{
    public static partial class TypeExtensions
    {
        public static MethodInfo GetInstanceMethod(
            this Type self,
            string name,
            bool includeNonPublic = false
        )
        {
            if (includeNonPublic)
            {
                return self.GetMethod(name, BINDING_FLAGS_INSTANCE | BINDING_FLAGS_ALL_ACCESS);
            }

            return self.GetMethod(name, BINDING_FLAGS_INSTANCE | BINDING_FLAGS_PUBLIC);
        }

        public static MethodInfo GetStaticMethod(
            this Type self,
            string name,
            bool includeNonPublic = false
        )
        {
            if (includeNonPublic)
            {
                return self.GetMethod(name, BINDING_FLAGS_STATIC | BINDING_FLAGS_ALL_ACCESS);
            }

            return self.GetMethod(name, BINDING_FLAGS_STATIC | BINDING_FLAGS_PUBLIC);
        }
    }
}
