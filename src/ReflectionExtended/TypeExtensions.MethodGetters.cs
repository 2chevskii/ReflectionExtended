using System.Reflection;
using System;

using JetBrains.Annotations;

namespace ReflectionExtended
{
    public static partial class TypeExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="name"></param>
        /// <param name="includeNonPublic"></param>
        /// <returns></returns>
        [CanBeNull]
        public static MethodInfo GetInstanceMethod([NotNull]this Type self, [NotNull]string name, bool includeNonPublic = false) =>
        self.GetMethod(
                       name,
                       BINDING_FLAGS_INSTANCE | (includeNonPublic ? BINDING_FLAGS_ALL_ACCESS : BINDING_FLAGS_PUBLIC)
                      );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="name"></param>
        /// <param name="includeNonPublic"></param>
        /// <returns></returns>
        [CanBeNull]
        public static MethodInfo GetStaticMethod([NotNull]this Type self, [NotNull]string name, bool includeNonPublic = false) =>
        self.GetMethod(
                       name,
                       BINDING_FLAGS_STATIC | (includeNonPublic ? BINDING_FLAGS_ALL_ACCESS : BINDING_FLAGS_PUBLIC)
                      );
    }
}
