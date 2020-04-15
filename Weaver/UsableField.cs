using PostSharp.Sdk.CodeModel;

namespace PostSharp.Community.ToString.Weaver
{
    /// <summary>
    /// A field in a type or one of its base types + a generic map to access that field from the original enhanced type.
    /// </summary>
    public class UsableField
    {
        public FieldDefDeclaration FieldDefinition { get; }
        public GenericMap MapToAccessTheFieldFromMostDerivedClass { get; }

        public UsableField(FieldDefDeclaration fieldDefinition, GenericMap mapToAccessTheFieldFromMostDerivedClass)
        {
            FieldDefinition = fieldDefinition;
            MapToAccessTheFieldFromMostDerivedClass = mapToAccessTheFieldFromMostDerivedClass;
        }
    }
}