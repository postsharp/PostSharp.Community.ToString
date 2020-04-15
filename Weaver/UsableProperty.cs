using PostSharp.Sdk.CodeModel;

namespace PostSharp.Community.ToString.Weaver
{
    /// <summary>
    /// A property in a type or one of its base types + a generic map to access that property from the original enhanced type.
    /// </summary>
    public class UsableProperty
    {
        public PropertyDeclaration PropertyDefinition { get; }
        public GenericMap MapToAccessThisPropertyFromMostDerivedClass { get; }

        public UsableProperty(PropertyDeclaration propertyDefinition, GenericMap mapToAccessThisPropertyFromMostDerivedClass)
        {
            PropertyDefinition = propertyDefinition;
            MapToAccessThisPropertyFromMostDerivedClass = mapToAccessThisPropertyFromMostDerivedClass;
        }
    }
}