using System;
using System.Collections.Generic;
using System.Reflection;

namespace ReflectionExtended
{
    public static class AppDomainExtensions
    {
        public static Assembly GetAssembly(
            this AppDomain self,
            string name,
            bool ignoreCase = false
        )
        {
            StringComparison comparisonMode =
                ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            var assemblies = self.GetAssemblies();

            for (var i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                var assemblyName = assembly.GetName().Name;

                if (assemblyName.Equals(name, comparisonMode))
                {
                    return assembly;
                }
            }

            return null;
        }

        public static Assembly GetAssemblyByPartialName(
            this AppDomain self,
            string name,
            bool ignoreCase = true
        )
        {
            var assemblies = self.GetAssemblies();

            for (var i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                var assemblyName = assembly.GetName().Name;

                if (assemblyName.Contains(name, ignoreCase))
                {
                    return assembly;
                }
            }

            return null;
        }

        public static IEnumerable<Assembly> GetAssembliesByPartialName(
            this AppDomain self,
            string name,
            bool ignoreCase = true
        )
        {
            var assemblies = self.GetAssemblies();
            var matchingAssemblies = new HashSet<Assembly>();

            for (var i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                var assemblyName = assembly.GetName().Name;

                if (assemblyName.Contains(name, ignoreCase))
                {
                    matchingAssemblies.Add(assembly);
                }
            }

            return matchingAssemblies;
        }
    }
}
