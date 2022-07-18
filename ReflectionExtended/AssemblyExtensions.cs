using System;
using System.Collections.Generic;
using System.Reflection;

namespace ReflectionExtended
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetInterfaces(
            this Assembly self,
            bool includePublic = true,
            bool includeInternal = false,
            bool includeNested = false
        )
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Type> GetAbstractClasses(this Assembly self)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Type> GetClasses(this Assembly self, bool includeAbstract = false)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Type> GetConcreteTypes(this Assembly self)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Type> GetStructs(this Assembly self)
        {
            throw new NotImplementedException();
        }
    }
}
