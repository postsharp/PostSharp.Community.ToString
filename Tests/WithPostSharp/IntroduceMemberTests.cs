using System;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using Xunit;

namespace PostSharp.Community.ToString.Tests.WithPostSharp
{
    public class IntroduceMemberTests
    {
        [Fact]
        public void TestIntroduceProperty()
        {
            Assert.Equal("{TargetClass; One: 1}", new TargetClass().ToString());
        }
    }

    [ToString, AspectClass]
    public class TargetClass
    {
        
    }

    [Serializable]
    public class AspectClass : InstanceLevelAspect
    {
        [IntroduceMember] 
        public int One => 1;
    }
}