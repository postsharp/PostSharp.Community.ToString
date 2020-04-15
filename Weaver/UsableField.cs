using PostSharp.Sdk.CodeModel;

namespace PostSharp.Community.ToString.Weaver
{
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