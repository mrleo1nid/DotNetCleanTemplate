using DotNetCleanTemplate.Domain.Common;

namespace DotNetCleanTemplate.UnitTests.Domain
{
    public class ValueObjectTests
    {
        private class TestValueObject : ValueObject
        {
            private readonly int _a;
            private readonly string _b;

            public TestValueObject(int a, string b)
            {
                _a = a;
                _b = b;
            }

            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return _a;
                yield return _b;
            }
        }

        [Fact]
        public void ValueObjects_WithSameComponents_AreEqual()
        {
            var v1 = new TestValueObject(1, "x");
            var v2 = new TestValueObject(1, "x");
            Assert.Equal(v1, v2);
            Assert.Equal(v1.GetHashCode(), v2.GetHashCode());
        }

        [Fact]
        public void ValueObjects_WithDifferentComponents_AreNotEqual()
        {
            var v1 = new TestValueObject(1, "x");
            var v2 = new TestValueObject(2, "x");
            var v3 = new TestValueObject(1, "y");
            Assert.NotEqual(v1, v2);
            Assert.NotEqual(v1, v3);
        }

        [Fact]
        public void ValueObject_Equals_NullOrOtherType()
        {
            var v1 = new TestValueObject(1, "x");
            Assert.False(v1.Equals(null));
            Assert.False(v1.Equals("string"));
        }

        [Fact]
        public void ValueObject_Equals_Self()
        {
            var v1 = new TestValueObject(1, "x");
            Assert.True(v1.Equals(v1));
        }
    }
}
