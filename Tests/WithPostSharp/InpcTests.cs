using PostSharp.Patterns.Model;
using Xunit;

namespace PostSharp.Community.ToString.Tests.WithPostSharp
{
    public class InpcTests
    {
        [Fact]
        public void TestNPC()
        {
            Assert.Equal("{DO; One: 1, Three: 3, Two: 2}", new DO().ToString());
        }
    }

    [NotifyPropertyChanged, ToString]
    public class DO
    {
        public int One { get; set; } = 1;
        public int Two = 2;
        public int Three => One + Two;
    }
}