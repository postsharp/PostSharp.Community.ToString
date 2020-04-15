using PostSharp.Sdk.CodeModel;

namespace PostSharp.Community.ToString.Weaver
{
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