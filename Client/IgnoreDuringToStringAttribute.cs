using PostSharp.Extensibility;

namespace PostSharp.Community.ToString
{
    /// <summary>
    /// The target field or property is excluded from the generation of code in ToString.
    /// </summary>
    [MulticastAttributeUsage(MulticastTargets.Field | MulticastTargets.Property)]
    public class IgnoreDuringToStringAttribute : MulticastAttribute
    {
        
    }
}