using PostSharp.Extensibility;
using Xunit;
#pragma warning disable 414

namespace PostSharp.Community.ToString.Tests
{
    [ToString(PropertyNameToValueSeparator = ":"), IgnoreDuringToString(AttributeTargetMemberAttributes = MulticastAttributes.Private)]
    public class OnlyPublic
    {
        public int X = 2;
        private int Y = 2;
        
        [Fact]
        public void TestSelf()
        {
            OnlyPublic o = new OnlyPublic();
            Assert.Equal("{OnlyPublic; X:2}", o.ToString());
        }
    }
}