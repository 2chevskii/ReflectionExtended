using System;
using System.Collections.Generic;
using System.Linq;

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
        [DataRow(typeof(SecondOrderDerivedClass), true, true, new[]{typeof(SecondOrderDerivedClass), typeof(DerivedClass),typeof(BaseClass),typeof(object)})]
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
    }
}
