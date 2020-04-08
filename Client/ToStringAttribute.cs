using PostSharp.Extensibility;

namespace PostSharp.Community.ToString
{
    /// <summary>
    /// Annotating a type with this attribute causes a ToString method to be generated in it. If the type already
    /// has a ToString method, it's kept and nothing is generated.
    /// </summary>
    [MulticastAttributeUsage(MulticastTargets.Class | MulticastTargets.Struct)]
    [RequirePostSharp("PostSharp.Community.ToString.Weaver", "ToStringTask")]
    public class ToStringAttribute : AbstractBaseToStringAttribute
    {
    }
}