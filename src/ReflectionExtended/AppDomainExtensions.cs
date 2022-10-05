using System;
using System.Collections.Generic;
using System.Reflection;

using JetBrains.Annotations;

using ReflectionExtended.Internal;

namespace ReflectionExtended
{
    /// <summary>
    /// Defines extension methods to work with <see cref="AppDomain"/> instances
    /// </summary>
    [PublicAPI]
    public static class AppDomainExtensions
    {
        /// <summary>
        /// Get specific assembly by name
        /// </summary>
        /// <param name="self"></param>
        /// <param name="name"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        [CanBeNull]
        public static Assembly GetAssembly(
            [NotNull]
            this AppDomain self,
            [NotNull]
            string name,
            bool ignoreCase = false
        )
        {
            StringComparison comparisonMode =
                ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            Assembly[] assemblies = self.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                string assemblyName = assembly.GetName().Name;

                if (assemblyName.Equals(name, comparisonMode))
                {
                    return assembly;
                }
            }

            return null;
        }

        /// <summary>
        /// Get first assembly containing given string in name
        /// </summary>
        /// <param name="self"></param>
        /// <param name="name"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        [CanBeNull]
        public static Assembly GetAssemblyByPartialName(
            [NotNull]
            this AppDomain self,
            [NotNull]
            string name,
            bool ignoreCase = true
        )
        {
            Assembly[] assemblies = self.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                string assemblyName = assembly.GetName().Name;

                if (assemblyName.Contains(name, ignoreCase))
                {
                    return assembly;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all assemblies with given string in Name
        /// </summary>
        /// <param name="self"></param>
        /// <param name="name"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<Assembly> GetAssembliesByPartialName(
            [NotNull]
            this AppDomain self,
            [NotNull]
            string name,
            bool ignoreCase = true
        )
        {
            Assembly[]        assemblies         = self.GetAssemblies();
            HashSet<Assembly> matchingAssemblies = new HashSet<Assembly>();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                string assemblyName = assembly.GetName().Name;

                if (assemblyName.Contains(name, ignoreCase))
                {
                    matchingAssemblies.Add(assembly);
                }
            }

            return matchingAssemblies;
        }
    }
}
