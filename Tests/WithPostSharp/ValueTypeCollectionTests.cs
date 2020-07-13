using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace PostSharp.Community.ToString.Tests.WithPostSharp
{
    public class ValueTypeCollectionTests
    {
        [Fact]
        public void TestToStringOnValueType()
        {
            Containing containing = new Containing();
            Assert.Equal("{Containing; Cvt: [1,2,3,4,...]}", containing.ToString());
        }
        [Fact]
        public void TestToStringOnValueType2()
        {
            Containing2 containing = new Containing2();
            Assert.Equal("{Containing2; Cvt: []}", containing.ToString());
        }

        [ToString]
        class Containing
        {
            public CollectionValueType Cvt { get; } = new CollectionValueType();
        }
        [ToString]
        class Containing2
        {
            public CollectionValueTypeWithValueTypeEnumerator Cvt { get; } = new CollectionValueTypeWithValueTypeEnumerator();
        }
        
        struct CollectionValueType : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                return new int[] {1, 2, 3, 4, 5, 6, 7}.GetEnumerator();
            }
        }

        struct CollectionValueTypeWithValueTypeEnumerator : IEnumerable<string>
        {
            public IEnumerator<string> GetEnumerator()
            {
                return en();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return en();
            }

            private IEnumerator<string> en()
            {
                return new ValueTypeEnumerator();
            }
        }
        
        struct ValueTypeEnumerator : IEnumerator<string>
        {
            public void Dispose()
            {
                
            }

            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {
            }

            public string Current { get; }

            object IEnumerator.Current => Current;
        }
    }
}