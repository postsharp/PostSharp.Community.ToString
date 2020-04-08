using System;
using PostSharp.Extensibility;

namespace PostSharp.Community.ToString
{
    /// <summary>
    /// Properties set on this attribute are used as default for all <see cref="ToStringAttribute"/> instances that
    /// don't have those properties set. Properties whose names start with "Attribute" are ignored on this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    [MulticastAttributeUsage(MulticastTargets.Assembly)]
    public class ToStringGlobalOptionsAttribute : AbstractBaseToStringAttribute
    {
        
    }
}