using System;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using Xunit;
// ReSharper disable EventNeverSubscribedTo.Global
#pragma warning disable 67

namespace PostSharp.Community.ToString.Tests.WithPostSharp
{
    [ToString]
    public class EventInterceptionTests
    {
        [Fact]
        public void TestIgnoreEvents()
        {
            Assert.Equal("{EventInterceptionTests}", new EventInterceptionTests().ToString());
        }

        public event Action FieldLikeEvent;
        
        [EIA]
        public event Action EnhancedFieldLikeEvent;
    }

    [Serializable]
    public class EIA : EventInterceptionAspect
    {
        public override void OnAddHandler(EventInterceptionArgs args)
        {
            base.OnAddHandler(args);
        }
    }
}