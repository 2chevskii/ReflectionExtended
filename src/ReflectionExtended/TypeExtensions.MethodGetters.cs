using System.Reflection;
using System;
using System.Linq.Expressions;
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
        public static MethodInfo GetInstanceMethod(
            [NotNull] this Type self,
            [NotNull] string name,
            bool includeNonPublic = false
        ) =>
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
        public static MethodInfo GetStaticMethod(
            [NotNull] this Type self,
            [NotNull] string name,
            bool includeNonPublic = false
        ) =>
            self.GetMethod(
                name,
                BINDING_FLAGS_STATIC | (includeNonPublic ? BINDING_FLAGS_ALL_ACCESS : BINDING_FLAGS_PUBLIC)
            );

        public static MemberInfo GetMember<T, R>(this T self, Expression<Func<T, R>> expression) { }
        public static FieldInfo GetField<T, R>(this T self, Expression<Func<T, R>> expression) { }
        public static PropertyInfo GetProperty<T, R>(this T self, Expression<Func<T, R>> expression) { }
        public static MethodInfo GetMethod<T, R>(this T self, Expression<Func<T, R>> expression) { }

        public static MemberInfo GetMember<R>(Expression<Func<R>> expression)
        {
            var memberExpr = expression.Body as MemberExpression;

            if (memberExpr != null)
            {
                return memberExpr.Member;
            }

            var callExpr = expression.Body as MethodCallExpression;

            return callExpr?.Method;
        }
        public static FieldInfo GetField<R>(Expression<Func<R>> expression) { }
        public static PropertyInfo GetProperty<R>(Expression<Func<R>> expression) { }
        public static MethodInfo GetMethod<R>(Expression<Func<R>> expression) { }

        public static bool IsField(this MemberInfo memberInfo) => memberInfo is FieldInfo;
        public static bool IsProperty(this MemberInfo memberInfo) => memberInfo is PropertyInfo;
        public static bool IsMethod(this MemberInfo memberInfo) => memberInfo is MethodInfo;

        /// <summary>
        ///
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"><paramref name="memberInfo"/> is not <see cref="MethodInfo"/></exception>
        public static MethodInfo AsMethod(this MemberInfo memberInfo) => (MethodInfo)memberInfo;

        /// <summary>
        ///
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"><paramref name="memberInfo"/> is not <see cref="PropertyInfo"/></exception>
        public static PropertyInfo AsProperty(this MemberInfo memberInfo) => (PropertyInfo)memberInfo;

        /// <summary>
        ///
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"><paramref name="memberInfo"/> is not <see cref="FieldInfo"/></exception>
        public static FieldInfo AsField(this MemberInfo memberInfo) => (FieldInfo)memberInfo;
    }
}
