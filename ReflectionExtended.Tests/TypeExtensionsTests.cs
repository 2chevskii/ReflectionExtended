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

        [TestMethod]
        public void AttributeTest()
        {
            var prop = typeof(AttrTarget).GetAttributeProperty<MockAttrAttribute, float>(attr => attr.Number);
            var field =
                typeof(AttrTarget).GetAttributeProperty<MockAttrAttribute, float>(
                    attr => attr.NumberField
                );
            Console.WriteLine("Property value: {0}", prop);
            Console.WriteLine("Field value: {0}", field);
        }
    }
}
