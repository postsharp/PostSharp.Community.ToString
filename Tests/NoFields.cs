using Xunit;

namespace PostSharp.Community.ToString.Tests
{
    [ToString]
    public class NoFields
    {
        [Fact]
        public void TestEmpty()
        {
            Assert.Equal("{NoFields}", new NoFields().ToString());
        }
    }
}