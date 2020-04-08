using System.Collections.Generic;
using PostSharp.Community.ToString;
using ReferencedDependency;

namespace PostSharp.Community.ToString.Tests.Fody.AssemblyToProcess
{
    [ToString]
    public class Child : Parent
    {
        public long InChild { get; set; }
    }

    [ToString]
    public class ComplexChild : ComplexParent
    {
        public long InChildNumber { get; set; }

        public string InChildText { get; set; }

        public IEnumerable<int> InChildCollection { get; set; }
    }

    [ToString]
    public class GenericChild : GenericParent<int>
    {
        public string InChild { get; set; }
    }
}