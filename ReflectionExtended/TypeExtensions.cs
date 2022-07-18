using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ReflectionExtended
{
    public static class TypeExtensions
    {
        const BindingFlags BINDING_FLAGS_PUBLIC     = BindingFlags.Public;
        const BindingFlags BINDING_FLAGS_NON_PUBLIC = BindingFlags.NonPublic;
        const BindingFlags BINDING_FLAGS_ALL_ACCESS = BindingFlags.Public | BindingFlags.NonPublic;
        const BindingFlags BINDING_FLAGS_INSTANCE   = BindingFlags.Instance;
        const BindingFlags BINDING_FLAGS_STATIC     = BindingFlags.Static;

        /// <summary>
        /// Check if current type is assignable to given type
        /// </summary>
        /// <param name="self">Current type</param>
        /// <param name="other">Target type</param>
        /// <returns></returns>
        public static bool Is(this Type self, Type other)
        {
            return other.IsAssignableFrom(self);
        }

        /// <summary>
        /// <inheritdoc cref="Is"/>
        /// </summary>
        /// <typeparam name="T">Current type</typeparam>
        /// <param name="self">Target type</param>
        /// <returns></returns>
        public static bool Is<T>(this Type self)
        {
            return self.Is(typeof(T));
        }

        /// <summary>
        /// Check if current type is equal to given type
        /// </summary>
        /// <param name="self">Current type</param>
        /// <param name="other">Target type</param>
        /// <returns></returns>
        public static bool IsExactly(this Type self, Type other)
        {
            return self == other;
        }

        /// <summary>
        /// <inheritdoc cref="IsExactly"/>
        /// </summary>
        /// <typeparam name="T">Current type</typeparam>
        /// <param name="self">Given type</param>
        /// <returns></returns>
        public static bool IsExactly<T>(this Type self)
        {
            return self.IsExactly(typeof(T));
        }

        /// <summary>
        /// Generic overload of <see cref="Type.IsAssignableFrom"/>
        /// </summary>
        /// <typeparam name="T">Source type</typeparam>
        /// <param name="self">Current type</param>
        /// <returns></returns>
        public static bool IsAssignableFrom<T>(this Type self)
        {
            return self.IsAssignableFrom(typeof(T));
        }

        public static bool IsAssignableTo<T>(this Type self)
        {
            return self.IsAssignableTo(typeof(T));
        }

#if !NET5_0_OR_GREATER
        /// <summary>
        /// Polyfill for .NET &lt; 5 which does the same as <see cref="Type.IsAssignableFrom"/> but with swapped arguments
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsAssignableTo(this Type self, Type other)
        {
            return other.IsAssignableFrom(self);
        }

#endif

        public static bool IsEnumerable(this Type self, bool notString = true)
        {
            if (notString && self == typeof(string))
                return false;

            return typeof(IEnumerable).IsAssignableFrom(self);
        }

        public static bool IsGenericCollection(this Type self)
        {
            return typeof(ICollection<>).IsAssignableFrom(self);
        }

        public static bool IsNonGenericCollection(this Type self)
        {
            return typeof(ICollection).IsAssignableFrom(self);
        }

        public static bool IsGenericList(this Type self)
        {
            return typeof(IList<>).IsAssignableFrom(self);
        }

        public static bool IsNonGenericList(this Type self)
        {
            return typeof(IList).IsAssignableFrom(self);
        }

        public static IEnumerable<Type> GetInheritanceChain(
            this Type type,
            bool includeSelf = true,
            bool includeObject = true
        )
        {
            var currentType = includeSelf ? type : type.BaseType;

            while (currentType is not null && (includeObject || currentType != typeof(object)))
            {
                yield return currentType;

                currentType = currentType.BaseType;
            }
        }

        public static IEnumerable<Type> GetNestedClasses(
            this Type self,
            BindingFlags bindingFlags = BindingFlags.Public,
            bool includeAbstract = true
        )
        {
            return self.GetNestedTypes(bindingFlags)
                       .Where(t => t.IsClass && (includeAbstract || !t.IsAbstract));
        }

        public static IEnumerable<Type> GetNestedAbstractClasses(
            this Type self,
            BindingFlags bindingFlags = BindingFlags.Public
        )
        {
            return self.GetNestedTypes(bindingFlags).Where(t => t.IsClass && t.IsAbstract);
        }

        public static IEnumerable<Type> GetNestedValueTypes(
            this Type self,
            BindingFlags bindingFlags = BindingFlags.Public
        )
        {
            return self.GetNestedTypes(bindingFlags).Where(t => t.IsValueType);
        }

        public static IEnumerable<Type> GetNestedInterfaces(
            this Type self,
            BindingFlags bindingFlags = BindingFlags.Public
        )
        {
            return self.GetNestedTypes(bindingFlags).Where(t => t.IsInterface);
        }

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            Assembly assembly,
            bool onlyExported = false
        )
        {
            Type[] assemblyTypes;

            if (onlyExported)
            {
                assemblyTypes = assembly.GetExportedTypes();
            }
            else
            {
                assemblyTypes = assembly.GetTypes();
            }

            return assemblyTypes.Where(t => t.Is(self) && t != self);
        }

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            IEnumerable<Assembly> assemblies,
            bool onlyExported = false
        )
        {
            return assemblies.SelectMany(assembly => self.GetDerivedTypes(assembly, onlyExported));
        }

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            params Assembly[] assemblies
        )
        {
            return self.GetDerivedTypes((IEnumerable<Assembly>) assemblies);
        }

        public static IEnumerable<Type> GetExportedDerivedTypes(
            this Type self,
            params Assembly[] assemblies
        )
        {
            return self.GetDerivedTypes(assemblies, true);
        }

        public static IEnumerable<Type> GetDerivedTypes(
            this Type self,
            AppDomain appDomain,
            bool onlyExported = false
        )
        {
            return appDomain.GetAssemblies()
                            .SelectMany(assembly => self.GetDerivedTypes(assembly, onlyExported));
        }

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

        public static TProperty GetAttributeProperty<TAttribute, TProperty>(
            this Type self,
            Expression<Func<TAttribute, TProperty>> propertySelectExpression
        ) where TAttribute : Attribute
        {
            var attribute = self.GetCustomAttribute<TAttribute>();

            if (attribute is null)
                throw new InvalidOperationException(
                    $"Type '{self.Name}' does not contain an attribute of type '{typeof(TAttribute).Name}'"
                );

            var memberExpression = propertySelectExpression.Body as MemberExpression;

            if (memberExpression is null)
                throw new ArgumentException(
                    "Given expression must be a member access expression",
                    nameof(propertySelectExpression)
                );

            object value;

            if (memberExpression.Member is PropertyInfo pi)
            {
                value = pi.GetValue(attribute);
            }
            else if (memberExpression.Member is FieldInfo fi)
            {
                value = fi.GetValue(attribute);
            }
            else
                throw new ArgumentException(
                    "Given expression must be a property or field access expression"
                );

            if (value is not TProperty propertyValue)
            {
                throw new ArgumentException();
            }

            return propertyValue;
        }

        public static Attribute GetCustomAttributeIgnoringInheritance(
            this Type self,
            Type attributeType,
            bool exactAttributeType = false
        )
        {
            if (self is null)
                throw new ArgumentNullException(nameof(self));

            if (!attributeType.Is<Attribute>())
            {
                throw new ArgumentException(
                    $"Given type must be derived from {typeof(Attribute).FullName}",
                    nameof(attributeType)
                );
            }

            var selfAttr = self.GetCustomAttribute(attributeType, true);

            if (selfAttr is not null &&
                (!exactAttributeType || selfAttr.GetType().IsExactly(attributeType)))
            {
                return selfAttr;
            }

            var inheritanceChain = self.GetInheritanceChain(false, false);

            foreach (Type ancestor in inheritanceChain)
            {
                var attr = ancestor.GetCustomAttribute(attributeType, false);

                if (attr is not null && (!exactAttributeType || attr.GetType() == attributeType))
                {
                    return attr;
                }
            }

            return null;
        }

        public static TAttribute GetCustomAttributeIgnoringInheritance<TAttribute>(
            this Type self,
            bool exactAttributeType = false
        ) where TAttribute : Attribute
        {
            return (TAttribute) self.GetCustomAttributeIgnoringInheritance(
                typeof(TAttribute),
                exactAttributeType
            );
        }

        public static IEnumerable<Attribute> GetCustomAttributesIgnoringInheritance(
            this Type self,
            Type attributeType,
            bool exactAttributeType = false
        )
        {
            if (self is null)
                throw new ArgumentNullException(nameof(self));

            if (!attributeType.Is<Attribute>())
            {
                throw new ArgumentException(
                    $"Given type must be derived from {typeof(Attribute).FullName}",
                    nameof(attributeType)
                );
            }

            var inheritanceChain = self.GetInheritanceChain(true, false);
            var list = new List<Attribute>();

            foreach (Type type in inheritanceChain)
            {
                var attributes = type.GetCustomAttributes(attributeType, false);

                foreach (object attribute in attributes)
                {
                    if (exactAttributeType && !attribute.GetType().IsExactly(attributeType))
                    {
                        continue;
                    }

                    list.Add((Attribute) attribute);
                }
            }

            return list;
        }

        public static IEnumerable<TAttribute> GetCustomAttributesIgnoringInheritance<TAttribute>(
            this Type self,
            bool exactAttributeType = false
        ) where TAttribute : Attribute

        {
            return self
                   .GetCustomAttributesIgnoringInheritance(typeof(TAttribute), exactAttributeType)
                   .Cast<TAttribute>();
        }

        public static bool HasAttribute(
            this Type self,
            Type attributeType,
            bool ignoreInheritance = false,
            bool exactAttributeType = false
        )
        {
            if (ignoreInheritance)
            {
                return self.GetCustomAttributeIgnoringInheritance(attributeType, exactAttributeType)
                           is not null;
            }

            if(exactAttributeType)
            {
                return self.GetCustomAttributes(attributeType)
                           .Any(a => a.GetType().IsExactly(attributeType));
            }

            return self.GetCustomAttributes(attributeType).Any();
        }

        public static bool HasAttribute<TAttribute>(
            this Type self,
            bool ignoreInheritance = false,
            bool exactAttributeType = false
        ) where TAttribute : Attribute
        {
            return self.HasAttribute(typeof(TAttribute), ignoreInheritance, exactAttributeType);
        }

        public static bool HasAttributeOnSelf(
            this Type self,
            Type attributeType,
            bool exactAttributeType = false
        )
        {
            if (self is null)
                throw new ArgumentNullException(nameof(self));

            if (!attributeType.Is<Attribute>())
                throw new ArgumentException(
                    $"Given type must be derived from {typeof(Attribute).FullName}",
                    nameof(attributeType)
                );

            var attribute = self.GetCustomAttribute(attributeType);

            if (attribute is null)
                return false;

            if (exactAttributeType)
                return attribute.GetType().IsExactly(attributeType);

            return true;
        }

        public static bool HasAttributeOnSelf<TAttribute>(
            this Type self,
            bool exactAttributeType = false
        ) where TAttribute : Attribute
        {
            return self.HasAttributeOnSelf(typeof(TAttribute), exactAttributeType);
        }
    }

}
