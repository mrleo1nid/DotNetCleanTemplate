using DotNetCleanTemplate.Domain.Common;

namespace DomainTests
{
    public class ValueObjectTests
    {
        private class DummyValueObject : ValueObject
        {
            public string Value { get; }

            public DummyValueObject(string value) => Value = value;

            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Value;
            }
        }

        [Fact]
        public void ValueObjectsWithSameValues_ShouldBeEqual()
        {
            var a = new DummyValueObject("test");
            var b = new DummyValueObject("test");
            Assert.Equal(a, b);
        }

        [Fact]
        public void ValueObjectsWithDifferentValues_ShouldNotBeEqual()
        {
            var a = new DummyValueObject("test1");
            var b = new DummyValueObject("test2");
            Assert.NotEqual(a, b);
        }
    }
}
