using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentAssertions;

using ReflectionExtended;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ReflectionExtended.Tests.Mocks;

namespace ReflectionExtended.Tests
{
    [TestClass]
    public class TypeExtensionsTests
    {
        [DataTestMethod]
        [DataRow(
            typeof(string),
            true,
            true,
            new[] {
                typeof(string),
                typeof(object)
            }
        )]
        [DataRow(
            typeof(string),
            false,
            true,
            new[] {typeof(object)}
        )]
        [DataRow(
            typeof(string),
            true,
            false,
            new[] {typeof(string)}
        )]
        [DataRow(
            typeof(BaseClass),
            true,
            true,
            new[] {
                typeof(BaseClass),
                typeof(object)
            }
        )]
        [DataRow(
            typeof(DerivedClass),
            true,
            true,
            new[] {
                typeof(DerivedClass),
                typeof(BaseClass),
                typeof(object)
            }
        )]
        [DataRow(
            typeof(SecondOrderDerivedClass),
            true,
            true,
            new[] {
                typeof(SecondOrderDerivedClass),
                typeof(DerivedClass),
                typeof(BaseClass),
                typeof(object)
            }
        )]
        [DataRow(
            typeof(SecondOrderDerivedClass),
            true,
            false,
            new[] {
                typeof(SecondOrderDerivedClass),
                typeof(DerivedClass),
                typeof(BaseClass)
            }
        )]
        [DataRow(
            typeof(SecondOrderDerivedClass),
            false,
            true,
            new[] {
                typeof(DerivedClass),
                typeof(BaseClass),
                typeof(object)
            }
        )]
        public void GetInheritanceChainTest(
            Type type,
            bool includeSelf,
            bool includeObject,
            IEnumerable<Type> expectedInheritanceChain
        )
        {
            var actualInheritanceChain = type.GetInheritanceChain(includeSelf, includeObject);
            actualInheritanceChain.Should()
                                  .Equal(
                                      expectedInheritanceChain,
                                      "Gotten inheritance chain is not equal to expected"
                                  );
        }

        [DataTestMethod]
        [DataRow(typeof(string), typeof(string), true)]
        [DataRow(typeof(string), typeof(object), true)]
        [DataRow(typeof(BaseClass), typeof(object), true)]
        [DataRow(typeof(BaseClass), typeof(DerivedClass), false)]
        [DataRow(typeof(DerivedClass), typeof(BaseClass), true)]
        [DataRow(typeof(SecondOrderDerivedClass), typeof(BaseClass), true)]
        [DataRow(typeof(SecondOrderDerivedClass), typeof(DerivedClass), true)]
        [DataRow(typeof(List<>), typeof(IList), true)]
        [DataRow(typeof(List<>), typeof(IList<string>), false)]
        [DataRow(typeof(List<string>), typeof(IList<string>), true)]
        [DataRow(typeof(List<string>), typeof(IEnumerable<string>), true)]
        [DataRow(typeof(List<string>), typeof(IEnumerable<int>), false)]
        [DataRow(typeof(List<string>), typeof(IEnumerable), true)]
        public void IsTest(Type type, Type other, bool expectedResult)
        {
            type.Is(other).Should().Be(expectedResult);
        }

        [TestMethod]
        public void IsGenericTest()
        {
            typeof(string).Is<string>().Should().BeTrue();
            typeof(string).Is<object>().Should().BeTrue();
            typeof(BaseClass).Is<object>().Should().BeTrue();
            typeof(BaseClass).Is<DerivedClass>().Should().BeFalse();
            typeof(DerivedClass).Is<BaseClass>().Should().BeTrue();
            typeof(SecondOrderDerivedClass).Is<BaseClass>().Should().BeTrue();
            typeof(SecondOrderDerivedClass).Is<DerivedClass>().Should().BeTrue();
            typeof(List<>).Is<IList>().Should().BeTrue();
            typeof(List<>).Is<IList<string>>().Should().BeFalse();
            typeof(List<string>).Is<IList<string>>().Should().BeTrue();
            typeof(List<string>).Is<IEnumerable<string>>().Should().BeTrue();
            typeof(List<string>).Is<IEnumerable<int>>().Should().BeFalse();
            typeof(List<string>).Is<IEnumerable>().Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(typeof(object), typeof(object), true)]
        [DataRow(typeof(string), typeof(object), false)]
        [DataRow(typeof(string), typeof(string), true)]
        [DataRow(typeof(BaseClass), typeof(object), false)]
        [DataRow(typeof(BaseClass), typeof(BaseClass), true)]
        [DataRow(typeof(DerivedClass), typeof(DerivedClass), true)]
        [DataRow(typeof(DerivedClass), typeof(BaseClass), false)]
        [DataRow(typeof(DerivedClass), typeof(SecondOrderDerivedClass), false)]
        [DataRow(typeof(List<>), typeof(IEnumerable<>), false)]
        [DataRow(typeof(List<string>), typeof(IEnumerable<string>), false)]
        [DataRow(typeof(List<string>), typeof(IList<string>), false)]
        [DataRow(typeof(List<string>), typeof(List<string>), true)]
        [DataRow(typeof(List<string>), typeof(List<int>), false)]
        public void IsExactlyTest(Type type, Type other, bool expectedResult)
        {
            type.IsExactly(other).Should().Be(expectedResult);
        }

        [TestMethod]
        public void IsExactlyGenericTest()
        {
            typeof(object).IsExactly<object>().Should().BeTrue();
            typeof(string).IsExactly<object>().Should().BeFalse();
            typeof(string).IsExactly<string>().Should().BeTrue();
            typeof(BaseClass).IsExactly<object>().Should().BeFalse();
            typeof(BaseClass).IsExactly<BaseClass>().Should().BeTrue();
            typeof(DerivedClass).IsExactly<DerivedClass>().Should().BeTrue();
            typeof(DerivedClass).IsExactly<BaseClass>().Should().BeFalse();
            typeof(DerivedClass).IsExactly<SecondOrderDerivedClass>().Should().BeFalse();
            typeof(List<>).IsExactly<IEnumerable>().Should().BeFalse();
            typeof(List<string>).IsExactly<IEnumerable<string>>().Should().BeFalse();
            typeof(List<string>).IsExactly<IList<string>>().Should().BeFalse();
            typeof(List<string>).IsExactly<List<string>>().Should().BeTrue();
            typeof(List<string>).IsExactly<List<int>>().Should().BeFalse();
        }

        [DataTestMethod]
        [DataRow(typeof(int),false)]
        [DataRow(typeof(IEnumerable),true)]
        [DataRow(typeof(IEnumerable<>),true)]
        [DataRow(typeof(int[]),true)]
        [DataRow(typeof(string[]),true)]
        [DataRow(typeof(long),false)]
        [DataRow(typeof(Type),false)]
        [DataRow(typeof(List<>),true)]
        [DataRow(typeof(List<int>),true)]
        [DataRow(typeof(Enum),false)]
        [DataRow(typeof(HashSet<char>),true)]
        public void IsEnumerableTest(Type type, bool expected)
        {
            type.IsEnumerable().Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(typeof(IList<>), true)]
        [DataRow(typeof(IList<bool>), true)]
        [DataRow(typeof(IList<int>), true)]
        [DataRow(typeof(ICollection<int>), true)]
        [DataRow(typeof(ICollection<>), true)]
        [DataRow(typeof(HashSet<string>), true)]
        [DataRow(typeof(LinkedList<string>), true)]
        [DataRow(typeof(object), false)]
        [DataRow(typeof(int), false)]
        [DataRow(typeof(string), false)]
        [DataRow(typeof(bool), false)]
        [DataRow(typeof(Enum), false)]
        [DataRow(typeof(Enumerable), false)]
        [DataRow(typeof(TestContext), false)]
        public void IsGenericCollectionTest(Type type, bool expected)
        {
            type.IsGenericCollection().Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(typeof(IList), true)]
        [DataRow(typeof(ICollection), true)]
        [DataRow(typeof(ArrayList), true)]
        [DataRow(typeof(Hashtable), true)]
        [DataRow(typeof(int), false)]
        [DataRow(typeof(string), false)]
        [DataRow(typeof(bool), false)]
        [DataRow(typeof(double), false)]
        [DataRow(typeof(Enum), false)]
        [DataRow(typeof(object), false)]
        [DataRow(typeof(IEnumerable), false)]
        [DataRow(typeof(IEnumerable<>), false)]
        public void IsNonGenericCollectionTest(Type type, bool expected)
        {
            type.IsNonGenericCollection().Should().Be(expected);
        }
    }
}
