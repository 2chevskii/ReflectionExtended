using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectionExtended
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> ExportedDelegates(this Assembly self) =>
        from type in self.GetExportedTypes()
        where type.IsDelegate()
        select type;

        public static IEnumerable<Type> Delegates(this Assembly self) =>
        from type in self.GetTypes() where type.IsDelegate() select type;

        public static IEnumerable<Type> ExportedClasses(this Assembly self) =>
        from type in self.GetExportedTypes() where type.IsClass && !type.IsDelegate() select type;

        public static IEnumerable<Type> Classes(this Assembly self) =>
        from type in self.GetTypes() where type.IsClass && !type.IsDelegate() select type;

        public static IEnumerable<Type> ExportedStructs(this Assembly self) =>
        from type in self.GetExportedTypes() where type.IsValueType select type;

        public static IEnumerable<Type> Structs(this Assembly self) =>
        from type in self.GetTypes() where type.IsValueType select type;

        public static IEnumerable<Type> ExportedInterfaces(this Assembly self) =>
        from type in self.GetExportedTypes() where type.IsInterface select type;

        public static IEnumerable<Type> Interfaces(this Assembly self) =>
        from type in self.GetTypes() where type.IsInterface select type;
        
        public static IEnumerable<Type> GetTypesWithAttribute(
            this Assembly self,
            Type          attributeType,
            bool          ignoreInheritance  = false,
            bool          exactAttributeType = false
        ) => self.GetTypes().WithAttribute( attributeType, ignoreInheritance, exactAttributeType );

        public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>(
            this Assembly self,
            bool          ignoreInheritance  = false,
            bool          exactAttributeType = false
        ) where TAttribute : Attribute => self.GetTypesWithAttribute(
            typeof( TAttribute ),
            ignoreInheritance,
            exactAttributeType
        );

        public static IEnumerable<Type> GetTypesWithAttributeOnSelf(
            this Assembly self,
            Type          attributeType,
            bool          exactAttributeType = false
        ) => self.GetTypes().WithAttributeOnSelf( attributeType, exactAttributeType );

        public static IEnumerable<Type> GetTypesWithAttributeOnSelf<TAttribute>(
            this Assembly self,
            bool          exactAttributeType = false
        ) where TAttribute : Attribute => self.GetTypesWithAttributeOnSelf(
            typeof( TAttribute ),
            exactAttributeType
        );
    }
}
