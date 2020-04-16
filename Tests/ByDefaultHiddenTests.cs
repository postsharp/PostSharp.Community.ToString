using System.Runtime.CompilerServices;
using Xunit;
#pragma warning disable 414

namespace PostSharp.Community.ToString.Tests
{
    public class ByDefaultHiddenTests
    {
        [Fact]
        public void TestHide()
        {
            Assert.Equal("{HideThem; Two: 2, Three: 3}", new HideThem().ToString());
        }
        
        [Fact]
        public void TestShow()
        {
            Assert.Equal("{ShowThem; One: 1, Two: 2, Three: 3}", new ShowThem().ToString());
        }
    }

    [ToString]
    public class HideThem
    {
        private int One = 1;
        [CompilerGenerated] 
        public int Two = 2;
        public int Three = 3;
    }
    
    [ToString(IncludePrivate = true)]
    public class ShowThem
    {
        private int One = 1;
        [CompilerGenerated] 
        public int Two = 2;
        public int Three = 3;
    }
}