using System;
using System.Collections.Generic;
using System.Linq;

namespace ReflectionExtended
{
    public static partial class EnumerableExtensions
    {
        public static bool AllAre(this IEnumerable<Type> self, Type target)
        {
            foreach (Type type in self)
            {
                if (!type.Is(target))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool AllAre<T>(this IEnumerable<Type> self)
        {
            return AllAre(self, typeof(T));
        }

        public static bool AllAreExactly(this IEnumerable<Type> self, Type target)
        {
            foreach (Type type in self)
            {
                if (!type.IsExactly(target))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool AllAreExactly<T>(this IEnumerable<Type> self)
        {
            return AllAreExactly(self, typeof(T));
        }

        public static bool AllAreAssignableFrom(this IEnumerable<Type> self, Type source)
        {
            foreach (Type type in self)
            {
                if (!type.IsAssignableFrom(source))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool AllAreAssignableFrom<T>(this IEnumerable<Type> self)
        {
            var source = typeof(T);

            return AllAreAssignableFrom(self, source);
        }

        public static bool AllAreAssignableTo(this IEnumerable<Type> self, Type target)
        {
            foreach (Type type in self)
            {
                if (!type.IsAssignableTo(target))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool AllAreAssignableTo<T>(this IEnumerable<Type> self)
        {
            return AllAreAssignableTo(self, typeof(T));
        }
    }
}
