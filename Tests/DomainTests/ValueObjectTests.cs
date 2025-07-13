using DotNetCleanTemplate.Domain.Common;

namespace DomainTests
{
    public class DummyValueObject : ValueObject
    {
        public string Value { get; }

        public DummyValueObject(string value) => Value = value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public class ValueObjectTests
    {
        [Fact]
        public void ValueObjects_WithSameValue_AreEqual()
        {
            var a = new DummyValueObject("test");
            var b = new DummyValueObject("test");
            Assert.Equal(a, b);
        }

        [Fact]
        public void ValueObjects_WithDifferentValue_AreNotEqual()
        {
            var a = new DummyValueObject("test1");
            var b = new DummyValueObject("test2");
            Assert.NotEqual(a, b);
        }

        [Fact]
        public void ValueObject_HashCode_IsEqual_ForEqualObjects()
        {
            var a = new DummyValueObject("abc");
            var b = new DummyValueObject("abc");
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ValueObject_NotEqualToNull()
        {
            var a = new DummyValueObject("abc");
            Assert.False(a.Equals(null));
        }
    }
}
