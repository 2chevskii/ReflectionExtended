using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace ReflectionExtended
{
    public static partial class TypeExtensions
    {
#region Property getter helpers

        public static object GetAttributeProperty(
            this Type self,
            Type      attributeType,
            string    propertyName,
            bool      ignoreInheritance  = false,
            bool      exactAttributeType = true
        )
        {
            Attribute attribute;

            if (ignoreInheritance)
            {
                attribute = self.GetCustomAttributeIgnoringInheritance( attributeType, exactAttributeType );
            }
            else
            {
                attribute = self.GetCustomAttributes( attributeType )
                                .FirstOrDefault(
                                    a => !exactAttributeType || a.GetType() == attributeType
                                );
            }

            if (attribute is null)
                throw new Exception( $"Attribute {attributeType} not found on type {self}" );

            foreach (MemberInfo memberInfo in attributeType.GetMember( propertyName ))
            {
                switch (memberInfo)
                {
                    case PropertyInfo property:
                        return property.GetValue( attribute );

                    case FieldInfo field:
                        return field.GetValue( attribute );
                }
            }

            throw new Exception( $"Property {propertyName} not found" );
        }

        public static TProperty GetAttributeProperty<TProperty>(
            this Type self,
            Type      attributeType,
            string    propertyName,
            bool      ignoreInheritance  = false,
            bool      exactAttributeType = true
        )
        {
            var value = self.GetAttributeProperty( attributeType, propertyName, ignoreInheritance, exactAttributeType );

            return (TProperty) Convert.ChangeType( value, typeof( TProperty ) );
        }

        public static object GetAttributeProperty<TAttribute>(
            this Type                            self,
            Expression<Func<TAttribute, object>> propertySelectExpression,
            bool                                 ignoreInheritance  = false,
            bool                                 exactAttributeType = true
        )
        {
            Attribute attribute;
            Type      attributeType = typeof( TAttribute );

            if (ignoreInheritance)
            {
                attribute = self.GetCustomAttributeIgnoringInheritance( attributeType, exactAttributeType );
            }
            else
            {
                attribute = self.GetCustomAttributes( attributeType )
                                .FirstOrDefault( a => !exactAttributeType || a.GetType() == attributeType );
            }

            if (attribute is null)
                throw new Exception( $"Attribute {attributeType} not found on type {self}" );

            var memberExpression = propertySelectExpression.Body as MemberExpression;

            if (memberExpression is null)
                throw new InvalidOperationException( "Given expression cannot be converted to MemberExpression" );

            switch (memberExpression.Member)
            {
                case PropertyInfo property:
                    return property.GetValue( attribute );

                case FieldInfo field:
                    return field.GetValue( attribute );
            }

            throw new Exception();

            /*
            return memberExpression.Member switch {
                PropertyInfo property => property.GetValue( attribute ),
                FieldInfo field       => field.GetValue( attribute ),
                // FIXME: Write more specific exceptions
                var _ => throw new Exception()
            };*/
        }

        public static TProperty GetAttributeProperty<TAttribute, TProperty>(
            this Type                               self,
            Expression<Func<TAttribute, TProperty>> propertySelectExpression,
            bool                                    ignoreInheritance  = false,
            bool                                    exactAttributeType = true
        )
        {
            Attribute attribute;
            Type      attributeType = typeof( TAttribute );

            if (ignoreInheritance)
            {
                attribute = self.GetCustomAttributeIgnoringInheritance( attributeType, exactAttributeType );
            }
            else
            {
                attribute = self.GetCustomAttributes( attributeType )
                                .FirstOrDefault( a => !exactAttributeType || a.GetType() == attributeType );
            }

            if (attribute is null)
                throw new Exception( $"Attribute {attributeType} not found on type {self}" );

            var memberExpression = propertySelectExpression.Body as MemberExpression;

            if (memberExpression is null)
                throw new InvalidOperationException( "Given expression cannot be converted to MemberExpression" );

            switch (memberExpression.Member)
            {
                case PropertyInfo property:
                    return (TProperty) Convert.ChangeType(
                        property.GetValue( attribute ),
                        typeof( TProperty )
                    );
                case FieldInfo field:
                    return (TProperty) Convert.ChangeType(
                        field.GetValue( attribute ),
                        typeof( TProperty )
                    );
            }

            throw new Exception();

            /*return memberExpression.Member switch {
                PropertyInfo property => (TProperty) Convert.ChangeType(
                    property.GetValue( attribute ),
                    typeof( TProperty )
                ),
                FieldInfo field => (TProperty) Convert.ChangeType(
                    field.GetValue( attribute ),
                    typeof( TProperty )
                ),
                // FIXME: Write more specific exceptions
                var _ => throw new Exception()
            };*/
        }

#endregion

        public static Attribute GetCustomAttributeIgnoringInheritance(
            this Type self,
            Type attributeType,
            bool exactAttributeType = false
        )
        {
            if (self is null)
                throw new ArgumentNullException( nameof( self ) );

            if (!attributeType.Is<Attribute>())
            {
                throw new ArgumentException(
                                            $"Given type must be derived from {typeof( Attribute ).FullName}",
                                            nameof( attributeType )
                                           );
            }

            var selfAttr = self.GetCustomAttribute( attributeType, true );

            if (selfAttr != null && (!exactAttributeType || selfAttr.GetType().IsExactly( attributeType )))
            {
                return selfAttr;
            }

            var inheritanceChain = self.GetInheritanceChain( false, false );

            foreach (Type ancestor in inheritanceChain)
            {
                var attr = ancestor.GetCustomAttribute( attributeType, false );

                if (attr != null && (!exactAttributeType || attr.GetType() == attributeType)) { return attr; }
            }

            return null;
        }

        public static TAttribute GetCustomAttributeIgnoringInheritance<TAttribute>(
            this Type self,
            bool exactAttributeType = false
        ) where TAttribute : Attribute =>
        (TAttribute) self.GetCustomAttributeIgnoringInheritance( typeof( TAttribute ), exactAttributeType );

        public static IEnumerable<Attribute> GetCustomAttributesIgnoringInheritance(
            this Type self,
            Type attributeType,
            bool exactAttributeType = false
        )
        {
            if (self is null)
                throw new ArgumentNullException( nameof( self ) );

            if (!attributeType.Is<Attribute>())
            {
                throw new ArgumentException(
                                            $"Given type must be derived from {typeof( Attribute ).FullName}",
                                            nameof( attributeType )
                                           );
            }
            
            return from type in self.GetInheritanceChain( true, false )
                   let attributes = type.GetCustomAttributes( attributeType, false )
                   from attribute in attributes
                   where !exactAttributeType || attribute.GetType().IsExactly( attributeType )
                   select (Attribute) attribute;
        }

        public static IEnumerable<TAttribute> GetCustomAttributesIgnoringInheritance<TAttribute>(
            this Type self,
            bool exactAttributeType = false
        ) where TAttribute : Attribute => self
                                          .GetCustomAttributesIgnoringInheritance(
                                               typeof( TAttribute ),
                                               exactAttributeType
                                              )
                                          .Cast<TAttribute>();

        public static bool HasAttribute(
            this Type self,
            Type attributeType,
            bool ignoreInheritance = false,
            bool exactAttributeType = false
        )
        {
            if (ignoreInheritance)
            {
                return self.GetCustomAttributeIgnoringInheritance( attributeType, exactAttributeType ) != null;
            }

            if (exactAttributeType)
            {
                return self.GetCustomAttributes( attributeType ).Any( a => a.GetType().IsExactly( attributeType ) );
            }

            return self.GetCustomAttributes( attributeType ).Any();
        }

        public static bool HasAttribute<TAttribute>(
            this Type self,
            bool ignoreInheritance = false,
            bool exactAttributeType = false
        ) where TAttribute : Attribute =>
        self.HasAttribute( typeof( TAttribute ), ignoreInheritance, exactAttributeType );

        public static bool HasAttributeOnSelf(this Type self, Type attributeType, bool exactAttributeType = false)
        {
            if (self is null)
                throw new ArgumentNullException( nameof( self ) );

            if (!attributeType.Is<Attribute>())
                throw new ArgumentException(
                                            $"Given type must be derived from {typeof( Attribute ).FullName}",
                                            nameof( attributeType )
                                           );

            var attribute = self.GetCustomAttribute( attributeType );

            if (attribute is null)
                return false;

            return !exactAttributeType || attribute.GetType().IsExactly( attributeType );
        }

        public static bool HasAttributeOnSelf<TAttribute>(this Type self, bool exactAttributeType = false)
        where TAttribute : Attribute => self.HasAttributeOnSelf( typeof( TAttribute ), exactAttributeType );
    }
}
