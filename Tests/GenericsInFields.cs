using System;
using System.Collections.Generic;
using Xunit;

namespace PostSharp.Community.ToString.Tests
{
    [ToString]
    public class GenericsInFields : MidField<float>
    {
        [Fact]
        public void TestItWorks()
        {
            var f = new GenericsInFields();
            Assert.Equal("{GenericsInFields; Y: null, X: null}", f.ToString());
        }
    }

    public class MidField<K> : LowField<List<K>>
    {
        public Tuple<K, K> Y;
    }

    public class LowField<T>
    {
        public T X;
    }
}