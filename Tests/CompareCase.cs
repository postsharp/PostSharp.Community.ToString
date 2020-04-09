#pragma warning disable 649
namespace PostSharp.Community.ToString.Tests
{
    public class CompareCase
    {
        public int Foo { get; }
        public string bar;
        private string[] baz;
        private float kak;

        public override string ToString()
        {
            return $"{{CompareCase; Foo:{Foo},bar:{bar},baz:{string.Join(",",baz)},kak:{kak}}}";
        }
    }
}