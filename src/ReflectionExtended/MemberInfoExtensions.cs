using System;
using System.Reflection;

namespace ReflectionExtended
{
    public static class MemberInfoExtensions
    {
        public static bool HasAttribute(this MemberInfo self, Type type)
        {
            if (!type.Is<Attribute>())
            {
                throw new ArgumentException(
                    $"Invalid type given: type must be inherited from {typeof(Attribute).FullName}",
                    nameof(type)
                );
            }

            return self.GetCustomAttribute(type) != null;
        }

        public static bool HasAttribute<T>(this MemberInfo self) where T : Attribute
        {
            return self.HasAttribute(typeof(T));
        }
    }
}
