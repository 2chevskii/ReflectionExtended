using System;

namespace ReflectionExtended.Tests.Mocks
{
    [MockAttr(Number = 42f, NumberField = 1337f)]
    public class AttrTarget
    {

    }

    public class MockAttrAttribute : Attribute
    {
        public float Number { get; set; }

        public float NumberField;
    }
}
