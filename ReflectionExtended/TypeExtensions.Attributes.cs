using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace ReflectionExtended
{
    public static partial class TypeExtensions
    {
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

            if (exactAttributeType)
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

            return !exactAttributeType || attribute.GetType().IsExactly(attributeType);
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
