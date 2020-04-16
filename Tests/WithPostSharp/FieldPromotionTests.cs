using System;
using PostSharp.Aspects;
using Xunit;

namespace PostSharp.Community.ToString.Tests.WithPostSharp
{
    [ToString]
    public class FieldPromotionTests
    {
        [Fact]
        public void TestSelf()
        {
            Assert.Equal("{FieldPromotionTests; Field: 1}", new FieldPromotionTests().ToString());
        }
        
        [LIA]
        public int Field;
    }

    [Serializable]
    public class LIA : LocationInterceptionAspect
    {
        public override void OnGetValue(LocationInterceptionArgs args)
        {
            args.Value = 1;
        }
    }
}