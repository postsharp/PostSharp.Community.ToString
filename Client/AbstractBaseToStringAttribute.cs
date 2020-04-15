using PostSharp.Extensibility;

namespace PostSharp.Community.ToString
{
    /// <summary>
    /// Base class for <see cref="ToStringAttribute"/> and <see cref="ToStringGlobalOptionsAttribute"/>.
    /// </summary>
    public abstract class AbstractBaseToStringAttribute : MulticastAttribute
    {
        private protected AbstractBaseToStringAttribute()
        {
        }

        /// <summary>
        /// The separator between a property or field name and value. The default is ":", as in "answer:42".
        /// </summary>
        public string PropertyNameToValueSeparator { get; set; }

        /// <summary>
        /// The separator between two properties or fields. The default is ",", as in "answer:42,another:54".
        /// </summary>
        public string PropertiesSeparator { get; set; }

        /// <summary>
        /// If true, the short name of the type is added to the ToString. The default is true, as in "MyType1; answer:42".
        /// </summary>
        public bool WriteTypeName { get; set; }

        /// <summary>
        /// If true, the ToString is result is wrapped in curly braces. The default is true, as in "{MyType1; answer:42}".
        /// </summary>
        public bool WrapWithBraces { get; set; } 
    }
}